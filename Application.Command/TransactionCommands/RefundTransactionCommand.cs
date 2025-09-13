using Application.Service.Contracts;
using Application.Service.Dtos.Transactions;
using Domain.Core.Entities;
using Domain.Core.Entities.AccountAggregate;
using Domain.Core.Entities.AccountAggregate.Exceptions;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.FinancialDocumentAggregate.Constants;
using Domain.Core.Entities.MerchantAggregate;
using Domain.Core.Entities.MerchantAggregate.Exceptions;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TransactionAggregate;
using Domain.Core.Enums;
using Domain.Core.Helper;
using Domain.Core.UnitOfWorkContracts;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using TransactionAggregate = Domain.Core.Entities.TransactionAggregate.Transaction;

namespace Application.Command.TransactionCommands;

public class RefundTransactionCommand : IRequest
{
    public RefundTransactionCommand(long financialDocumentId, int? merchantId, decimal amount, RefundReason reason, string description, int tenantId, int? merchantBranchId = null, bool isMerchant = false)
    {
        FinancialDocumentId = financialDocumentId;
        MerchantId = merchantId;
        Amount = amount;
        Description = description;
        Reason = reason;
        TenantId = tenantId;
        MerchantBranchId = merchantBranchId;
        IsMerchant = isMerchant;
    }

    public long FinancialDocumentId { get; set; }
    public int? MerchantId { get; set; }
    public int? MerchantBranchId { get; set; }
    public decimal Amount { get; set; }
    public RefundReason Reason { get; set; }
    public string Description { get; set; }
    public int TenantId { get; }
    public bool IsMerchant { get; }

    public class RefundTransactionCommandHandler : IRequestHandler<RefundTransactionCommand>
    {
        private readonly IFinancialDocumentRepository _financialDocumentRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IMerchantRepository _merchantRepository;
        private readonly IApplicationDbContextUnitOfWork _unitOfWork;
        private readonly ITransactionService _transactionService;
        private readonly IAccountRepository _accountRepository;

        public RefundTransactionCommandHandler(
            IFinancialDocumentRepository financialDocumentRepository,
            IApplicationDbContextUnitOfWork unitOfWork,
            ITransactionRepository transactionRepository,
            ITransactionService transactionService,
            IMerchantRepository merchantRepository,
            IAccountRepository accountRepository)
        {
            _financialDocumentRepository = financialDocumentRepository;
            _unitOfWork = unitOfWork;
            _transactionRepository = transactionRepository;
            _transactionService = transactionService;
            _merchantRepository = merchantRepository;
            _accountRepository = accountRepository;
        }

        public async Task Handle(RefundTransactionCommand command, CancellationToken cancellationToken)
        {
            if (command.IsMerchant && (command.MerchantId == null || command.MerchantId == 0))
            {
                throw new ArgumentValidationException(nameof(command.MerchantId), "شناسه پذیرنده الزامی می باشد");
            }

            if (!command.IsMerchant && (command.MerchantBranchId == null || command.MerchantBranchId == 0))
            {
                throw new ArgumentValidationException(nameof(command.MerchantBranchId), "شناسه شعبه پذیرنده الزامی می باشد");
            }

            if (command.MerchantBranchId.HasValue && command.MerchantBranchId > 0)
            {
                var merchantBranch = await _merchantRepository.GetBranchAsync(command.MerchantBranchId.Value) ?? throw new MerchantBranchNotFoundException("شعبه پذیرنده یافت نشد");
                command.MerchantId = merchantBranch.MerchantId;
            }

            var financialDocument = await _financialDocumentRepository.GetByIdAsync(command.FinancialDocumentId)
                ?? throw new AccountNotFoundException($"سند مالی یافت نشد");

            if (financialDocument.Type != FinancialDocumentType.Purchase || financialDocument.Type == FinancialDocumentType.Refund)
            {
                throw new ArgumentValidationException(nameof(command.FinancialDocumentId), "امکان عودت وجه این سند مالی وجود ندارد");
            }

            if (command.TenantId != financialDocument.TenantId)
            {
                throw new TenantForbiddenException();
            }

            if (command.MerchantId != financialDocument.ToBusinessIdentityId)
            {
                throw new ArgumentValidationException(nameof(command.MerchantId), "این پذیرنده امکان عودت وجه این سند مالی را ندارد");
            }

            if (command.MerchantBranchId != null && command.MerchantBranchId != financialDocument.MerchantBranchId)
            {
                throw new ArgumentValidationException(nameof(command.MerchantId), "این شعبه پذیرنده امکان عودت وجه این سند مالی را ندارد");
            }

            if (financialDocument.RefundType == FinancialDocumentRefundType.CompleteRefund)
            {
                throw new ArgumentValidationException(nameof(financialDocument.RefundType), "این سند به طور کامل عودت وجه شده است");
            }

            var refundFinancialDocuments = await _financialDocumentRepository.GetRefundsByParentIdAsync(command.FinancialDocumentId);

            if (refundFinancialDocuments.Sum(x => x.Amount) >= financialDocument.Amount)
            {
                throw new ArgumentValidationException(nameof(command.FinancialDocumentId), "مبلغ این خرید قبلا به طور کامل عودت داده شده است. امکان درخواست مجدد وجود ندارد");
            }

            if (refundFinancialDocuments.Sum(x => x.Amount) + command.Amount > financialDocument.Amount)
            {
                throw new ArgumentValidationException(nameof(command.Amount), "مجموع مبلغ عودت نمی تواند بیشتر از مبلغ خرید باشد");
            }

            if (refundFinancialDocuments.Sum(x => x.Amount) + command.Amount == financialDocument.Amount)
            {
                financialDocument.SetRefundType(FinancialDocumentRefundType.CompleteRefund);
            }
            else
            {
                financialDocument.SetRefundType(FinancialDocumentRefundType.PartiallyRefund);
            }

            var businessIdentityIds = new List<int>() { financialDocument.FromBusinessIdentityId, command.TenantId };

            var accounts = await _accountRepository.GetByBusinessIdentityIds(businessIdentityIds, command.TenantId);

            TransactionAggregate? refundCashTransaction = null;

            using (TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var refundFinancialDocumentPayments = CreateRefundFinancialDocumentPayments(command.Amount, refundFinancialDocuments, financialDocument);

                    foreach (var financialDocumentPayment in refundFinancialDocumentPayments.OrderBy(x => x.Type))
                    {
                        if (financialDocumentPayment.Type == FinancialDocumentPaymentType.Credit)
                        {
                            var transaction = await _transactionRepository.GetByIdAsync(financialDocumentPayment.TransactionId);

                            var refundCreditTransaction = await CreateRefundCreditTransaction(transaction, financialDocumentPayment.Amount, transactionScope, cancellationToken);

                            financialDocumentPayment.SetTransaction(refundCreditTransaction);
                        }

                        else if (financialDocumentPayment.Type == FinancialDocumentPaymentType.Cash)
                        {
                            if (refundCashTransaction == null)
                            {
                                var cashAndPrepaymentAmount = refundFinancialDocumentPayments.Where(x => x.Type == FinancialDocumentPaymentType.Prepayment || x.Type == FinancialDocumentPaymentType.Cash).Sum(x => x.Amount);
                                refundCashTransaction = await CreateTenantPurchaseToCustomerCashWalletTransaction(cashAndPrepaymentAmount, financialDocument, accounts, transactionScope);
                            }

                            financialDocumentPayment.SetTransaction(refundCashTransaction);
                        }

                        else if (financialDocumentPayment.Type == FinancialDocumentPaymentType.Prepayment)
                        {
                            if (refundCashTransaction == null)
                            {
                                var cashAndPrepaymentAmount = refundFinancialDocumentPayments.Where(x => x.Type == FinancialDocumentPaymentType.Prepayment || x.Type == FinancialDocumentPaymentType.Cash).Sum(x => x.Amount);
                                refundCashTransaction = await CreateTenantPurchaseToCustomerCashWalletTransaction(cashAndPrepaymentAmount, financialDocument, accounts, transactionScope);
                            }

                            financialDocumentPayment.SetTransaction(refundCashTransaction);
                        }
                    }

                    var refundFinancialDocument = CreateRefundFinancialDocument(refundFinancialDocumentPayments, command, financialDocument);

                    await _financialDocumentRepository.AddAsync(refundFinancialDocument);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    transactionScope.Complete();
                }
                catch (Exception)
                {
                    transactionScope.Dispose();
                    throw;
                }
            }
        }

        private FinancialDocument CreateRefundFinancialDocument(List<FinancialDocumentPayment> financialDocumentPayments, RefundTransactionCommand command, FinancialDocument financialDocument)
        {
            var refundFinancialDocument = new FinancialDocument(financialDocument.ToBusinessIdentityId,
                financialDocument.FromBusinessIdentityId, financialDocument.TenantId, command.Amount,
                financialDocument.PaymentId, FinancialDocumentType.Refund, FinancialDocumentState.Completed, null,
                FinancialDocumentType.Refund.GetEnumDescription(), financialDocument.MerchantBranchId,
                financialDocument.TenantMerchantContractId, financialDocument.TenantPlatformContractId);

            refundFinancialDocument.SetParentId(financialDocument.Id);
            refundFinancialDocument.Refund(command.Reason, command.Description);
            refundFinancialDocument.SetFinancialDocumentPayments(financialDocumentPayments);
            return refundFinancialDocument;
        }

        private List<FinancialDocumentPayment> CreateRefundFinancialDocumentPayments(decimal amount, List<FinancialDocument> refundFinancialDocuments, FinancialDocument financialDocument)
        {
            var refundFinancialDocumentPayments = new List<FinancialDocumentPayment>();
            var totalAmount = amount;

            var refundCreditAmount = refundFinancialDocuments.SelectMany(x => x.FinancialDocumentPayments).Where(x => x.Type == FinancialDocumentPaymentType.Credit).Sum(x => x.Amount);
            var refundCashAmount = refundFinancialDocuments.SelectMany(x => x.FinancialDocumentPayments).Where(x => x.Type == FinancialDocumentPaymentType.Cash).Sum(x => x.Amount);
            var refundPrepaymentAmount = refundFinancialDocuments.SelectMany(x => x.FinancialDocumentPayments).Where(x => x.Type == FinancialDocumentPaymentType.Prepayment).Sum(x => x.Amount);

            foreach (var financialDocumentPayment in financialDocument.FinancialDocumentPayments.OrderBy(x => x.Type))
            {
                if (totalAmount == 0)
                {
                    break;
                }

                decimal transactionAmount = 0;

                if (financialDocumentPayment.Type == FinancialDocumentPaymentType.Credit)
                {
                    if (totalAmount >= financialDocumentPayment.Amount - refundCreditAmount)
                    {
                        transactionAmount = financialDocumentPayment.Amount - refundCreditAmount;
                        totalAmount -= transactionAmount;
                    }
                    else
                    {
                        transactionAmount = totalAmount;
                        totalAmount = 0;
                    }

                    refundFinancialDocumentPayments.Add(new FinancialDocumentPayment(financialDocumentPayment.ToBusinessIdentityId, financialDocumentPayment.FromBusinessIdentityId, financialDocumentPayment.Type, transactionAmount, financialDocumentPayment.WalletId, financialDocumentPayment.WalletContractId, financialDocumentPayment.TransactionId, financialDocumentPayment.PaymentDetailId));
                }

                if (financialDocumentPayment.Type == FinancialDocumentPaymentType.Cash)
                {
                    if (totalAmount >= financialDocumentPayment.Amount - refundCashAmount)
                    {
                        transactionAmount = financialDocumentPayment.Amount - refundCashAmount;
                        totalAmount -= transactionAmount;
                    }
                    else
                    {
                        transactionAmount = totalAmount;
                        totalAmount = 0;
                    }

                    refundFinancialDocumentPayments.Add(new FinancialDocumentPayment(financialDocumentPayment.ToBusinessIdentityId, financialDocumentPayment.FromBusinessIdentityId, financialDocumentPayment.Type, transactionAmount, financialDocumentPayment.WalletId, financialDocumentPayment.WalletContractId, financialDocumentPayment.TransactionId, financialDocumentPayment.PaymentDetailId));
                }

                if (financialDocumentPayment.Type == FinancialDocumentPaymentType.Prepayment)
                {
                    if (totalAmount >= financialDocumentPayment.Amount - refundPrepaymentAmount)
                    {
                        transactionAmount = financialDocumentPayment.Amount - refundPrepaymentAmount;
                        totalAmount -= transactionAmount;
                    }
                    else
                    {
                        transactionAmount = totalAmount;
                        totalAmount = 0;
                    }

                    refundFinancialDocumentPayments.Add(new FinancialDocumentPayment(financialDocumentPayment.ToBusinessIdentityId, financialDocumentPayment.FromBusinessIdentityId, financialDocumentPayment.Type, transactionAmount, financialDocumentPayment.WalletId, financialDocumentPayment.WalletContractId, financialDocumentPayment.TransactionId, financialDocumentPayment.PaymentDetailId));
                }
            }

            return refundFinancialDocumentPayments;
        }

        private async Task<TransactionAggregate> CreateRefundCreditTransaction(TransactionAggregate existingTransaction, decimal amount, TransactionScope transactionScope, CancellationToken cancellationToken)
        {
            var newTransactions = new List<TransactionDto>()
            {
                new()
                {
                    FromAccountId = existingTransaction.ToAccountId,
                    ToAccountId =  existingTransaction.FromAccountId,
                    TenantId = existingTransaction.TenantId,
                    Amount = amount,
                    TransactionType = TransactionType.Refund,
                    Description = TransactionDescriptionConstants.REFUND,
                }
            };

            var transactions = await _transactionService.CreateTransactionAsync(newTransactions, transactionScope);
            return transactions.FirstOrDefault();
        }

        private async Task<TransactionAggregate> CreateTenantPurchaseToCustomerCashWalletTransaction(decimal cashAndPrepaymentAmount, FinancialDocument financialDocument, List<Account> accounts, TransactionScope transactionScope)
        {
            var tenantPurchaseAccount = accounts.FirstOrDefault(x => x.BusinessIdentityId == financialDocument.TenantId
                && x.Type == AccountType.Purchase);

            var customerCashWalletAccount = accounts.FirstOrDefault(x => x.BusinessIdentityId == financialDocument.FromBusinessIdentityId
                && x.Type == AccountType.CashWallet);

            var newTransactions = new List<TransactionDto>()
            {
                new()
                {
                    FromAccountId = tenantPurchaseAccount.Id,
                    ToAccountId =  customerCashWalletAccount.Id,
                    TenantId = financialDocument.TenantId,
                    Amount = cashAndPrepaymentAmount,
                    TransactionType = TransactionType.Refund,
                    Description = TransactionDescriptionConstants.REFUND,
                }
            };

            var transactions = await _transactionService.CreateTransactionAsync(newTransactions, transactionScope);
            return transactions.FirstOrDefault();
        }
    }
}