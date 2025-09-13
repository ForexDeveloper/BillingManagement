using Application.Command.TransactionCommands.Dtos;
using Application.Service.Contracts;
using Application.Service.Dtos.Transactions;
using Domain.Core.Entities.AccountAggregate;
using Domain.Core.Entities.AccountAggregate.Exceptions;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.FinancialDocumentAggregate.Constants;
using Domain.Core.Entities.TransactionAggregate;
using Domain.Core.Enums;
using Domain.Core.UnitOfWorkContracts;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Command.TransactionCommands;

public class ReverseTransactionCommand : IRequest<List<ReverseTransactionDto>>
{
    public ReverseTransactionCommand(List<long> paymentIds)
    {
        PaymentIds = paymentIds;
    }

    public List<long> PaymentIds { get; private set; }

    public class ReverseTransactionCommandHandler : IRequestHandler<ReverseTransactionCommand, List<ReverseTransactionDto>>
    {
        private readonly IFinancialDocumentRepository _financialDocumentRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IApplicationDbContextUnitOfWork _unitOfWork;
        private readonly ITransactionService _transactionService;

        public ReverseTransactionCommandHandler(
            IFinancialDocumentRepository financialDocumentRepository,
            IApplicationDbContextUnitOfWork unitOfWork,
            IAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            ITransactionService transactionService)
        {
            _financialDocumentRepository = financialDocumentRepository;
            _unitOfWork = unitOfWork;
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _transactionService = transactionService;
        }

        public async Task<List<ReverseTransactionDto>> Handle(ReverseTransactionCommand request, CancellationToken cancellationToken)
        {
            var reverseTransactionDtos = new List<ReverseTransactionDto>();

            foreach (var paymentId in request.PaymentIds)
            {
                var financialDocument = await _financialDocumentRepository.GetAsync(paymentId)
                    ?? throw new AccountNotFoundException($"سند مالی برای شناسه پرداخت {paymentId} یافت نشد");

                if (financialDocument.State == FinancialDocumentState.Reverse)
                {
                    reverseTransactionDtos.Add(new ReverseTransactionDto(paymentId, true));
                    continue;
                }

                financialDocument.SetState(FinancialDocumentState.Reverse);

                using (TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {
                        await CreateReverseTransactions(cancellationToken, financialDocument, transactionScope);

                        _financialDocumentRepository.Update(financialDocument);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                        transactionScope.Complete();

                        reverseTransactionDtos.Add(new ReverseTransactionDto(paymentId, true));
                    }
                    catch (Exception)
                    {
                        transactionScope.Dispose();

                        reverseTransactionDtos.Add(new ReverseTransactionDto(paymentId, false));
                        continue;
                    }
                }
            }

            return reverseTransactionDtos;
        }

        private async Task CreateReverseTransactions(CancellationToken cancellationToken, FinancialDocument financialDocument, TransactionScope transactionScope)
        {
            var creditFinancialDocumentPayment = financialDocument.FinancialDocumentPayments.FirstOrDefault(x => x.Type == FinancialDocumentPaymentType.Credit);
            var cashFinancialDocumentPayment = financialDocument.FinancialDocumentPayments.Where(x => x.Type == FinancialDocumentPaymentType.Cash || x.Type == FinancialDocumentPaymentType.Prepayment);

            var businessIdentityIds = new List<int>() { financialDocument.FromBusinessIdentityId, financialDocument.TenantId };
            var accounts = await _accountRepository.GetByBusinessIdentityIds(businessIdentityIds, financialDocument.TenantId);

            var customerBankAccount = accounts.FirstOrDefault(x => x.BusinessIdentityId == financialDocument.FromBusinessIdentityId
                    && x.Type == AccountType.Bank);

            var customerCashWalletAccount = accounts.FirstOrDefault(x => x.BusinessIdentityId == financialDocument.FromBusinessIdentityId
                    && x.Type == AccountType.CashWallet);

            var tenantBankAccount = accounts.FirstOrDefault(x => x.BusinessIdentityId == financialDocument.TenantId
                    && x.Type == AccountType.Bank);

            var tenantPurchaseAccount = accounts.FirstOrDefault(x => x.BusinessIdentityId == financialDocument.TenantId
                    && x.Type == AccountType.Purchase);

            var newTransactions = new List<TransactionDto>();

            if (creditFinancialDocumentPayment != null)
            {
                var transaction = await _transactionRepository.GetByIdAsync(creditFinancialDocumentPayment.TransactionId);

                var newTransaction = new TransactionDto()
                {
                    FromAccountId = transaction.ToAccountId,
                    ToAccountId = transaction.FromAccountId,
                    TenantId = transaction.TenantId,
                    Amount = transaction.Amount,
                    TransactionType = TransactionType.Reverse,
                    Description = TransactionDescriptionConstants.REVERSE,
                };

                newTransactions.Add(newTransaction);
            }

            if (cashFinancialDocumentPayment != null && cashFinancialDocumentPayment.Any())
            {
                decimal cashAndPrepaymentAmount = cashFinancialDocumentPayment.Sum(x => x.Amount);

                var newTransaction = new TransactionDto()
                {
                    FromAccountId = tenantPurchaseAccount.Id,
                    ToAccountId = customerCashWalletAccount.Id,
                    TenantId = financialDocument.TenantId,
                    Amount = cashAndPrepaymentAmount,
                    TransactionType = TransactionType.Reverse,
                    Description = TransactionDescriptionConstants.REVERSE,
                    Children = [
                        new()
                        {
                            FromAccountId = customerCashWalletAccount.Id,
                            ToAccountId = tenantBankAccount.Id,
                            TenantId = financialDocument.TenantId,
                            TransactionType = TransactionType.Transfer,
                            Description = TransactionDescriptionConstants.REVERSE,
                            Amount = cashAndPrepaymentAmount,
                            Children = [
                                new()
                                {
                                    FromAccountId = tenantBankAccount.Id,
                                    ToAccountId = customerBankAccount.Id,
                                    TenantId = financialDocument.TenantId,
                                    TransactionType = TransactionType.Transfer,
                                    Description = TransactionDescriptionConstants.REVERSE,
                                    Amount = cashAndPrepaymentAmount
                                }
                            ]
                        }
                    ]
                };

                newTransactions.Add(newTransaction);
            }

            await _transactionService.CreateTransactionAsync(newTransactions, transactionScope);
        }
    }
}