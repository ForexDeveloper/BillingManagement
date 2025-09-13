using Application.Command.TransactionCommands.Dtos;
using Application.Service.Contracts;
using Application.Service.Dtos.FinancialDocuments;
using Application.Service.Dtos.Transactions;
using Domain.Core.Entities.AccountAggregate;
using Domain.Core.Entities.CustomerAggregate;
using Domain.Core.Entities.CustomerAggregate.Exceptions;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.FinancialDocumentAggregate.Constants;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.TenantAggregate.Exceptions;
using Domain.Core.Entities.TenantPlatformContractAggregate;
using Domain.Core.Entities.TenantPlatformContractAggregate.Exceptions;
using Domain.Core.Entities.WalletAggregate;
using Domain.Core.Enums;
using Domain.Core.Helper;
using Domain.Core.UnitOfWorkContracts;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using TransactionAggregate = Domain.Core.Entities.TransactionAggregate.Transaction;

namespace Application.Command.TransactionCommands;

public class SetPaymentTransactionCommand : IRequest<List<SetTransactionDto>>
{
    public SetPaymentTransactionCommand(int tenantId, int customerId, decimal amount, long paymentId, PaymentServiceType paymentServiceType, List<PaymentDetailDto> paymentDetails)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        Amount = amount;
        PaymentId = paymentId;
        PaymentServiceType = paymentServiceType;
        PaymentDetails = paymentDetails;
    }

    public int TenantId { get; set; }
    public int CustomerId { get; private set; }
    public decimal Amount { get; private set; }
    public long PaymentId { get; private set; }
    public PaymentServiceType PaymentServiceType { get; private set; }
    public List<PaymentDetailDto> PaymentDetails { get; set; }

    public class SetPaymentTransactionCommandHandler : IRequestHandler<SetPaymentTransactionCommand, List<SetTransactionDto>>
    {
        private readonly IWalletRepository _walletRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly IFinancialDocumentRepository _financialDocumentRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ITransactionService _transactionService;
        private readonly ITenantPlatformContractRepository _tenantPlatformContractRepository;
        private readonly IApplicationDbContextUnitOfWork _unitOfWork;
        private readonly IAccountRepository _accountRepository;

        public SetPaymentTransactionCommandHandler(
            ITenantRepository tenantRepository,
            IFinancialDocumentRepository financialDocumentRepository,
            IWalletRepository walletRepository,
            ICustomerRepository customerRepository,
            ITransactionService transactionService,
            ITenantPlatformContractRepository tenantPlatformContractRepository,
            IApplicationDbContextUnitOfWork unitOfWork,
            IAccountRepository accountRepository)
        {
            _tenantRepository = tenantRepository;
            _financialDocumentRepository = financialDocumentRepository;
            _walletRepository = walletRepository;
            _customerRepository = customerRepository;
            _transactionService = transactionService;
            _tenantPlatformContractRepository = tenantPlatformContractRepository;
            _unitOfWork = unitOfWork;
            _accountRepository = accountRepository;
        }

        public async Task<List<SetTransactionDto>> Handle(SetPaymentTransactionCommand command, CancellationToken cancellationToken)
        {
            FinancialDocument existingFinancialDocument = await _financialDocumentRepository.GetAsync(command.PaymentId);
            if (existingFinancialDocument != null)
            {
                return existingFinancialDocument.FinancialDocumentPayments.Select(x => new SetTransactionDto
                {
                    PaymentDetailId = x.PaymentDetailId.Value,
                    TransactionId = x.TransactionId
                }).ToList();
            }

            await ValidateInputData(command);
            await SetCashWallet(command);

            var tenant = await _tenantRepository.GetAsync(command.TenantId) ?? throw new TenantNotFoundException("مالک زیر ساخت یافت نشد");

            var businessIdentityIds = new List<int>() { command.CustomerId, command.TenantId };

            var accounts = await _accountRepository.GetByBusinessIdentityIds(businessIdentityIds, command.TenantId);

            var customerBankAccount = accounts.FirstOrDefault(x => x.BusinessIdentityId == command.CustomerId
                    && x.Type == AccountType.Bank);

            var customerCashWalletAccount = accounts.FirstOrDefault(x => x.BusinessIdentityId == command.CustomerId
                    && x.Type == AccountType.CashWallet);

            var tenantBankAccount = accounts.FirstOrDefault(x => x.BusinessIdentityId == command.TenantId
                    && x.Type == AccountType.Bank);

            var tenantLoanAccount = accounts.FirstOrDefault(x => x.BusinessIdentityId == command.TenantId
                    && x.Type == AccountType.Loan);

            using (TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var newChildTransactions = new List<TransactionDto>();

                    if (command.PaymentServiceType == PaymentServiceType.OperationFee)
                    {
                        var newChildTransaction = new TransactionDto()
                        {
                            FromAccountId = customerCashWalletAccount.Id,
                            ToAccountId = tenantLoanAccount.Id,
                            TenantId = command.TenantId,
                            Amount = command.Amount,
                            TransactionType = TransactionType.OperationalFee,
                            Description = TransactionDescriptionConstants.OPERATIONAL_FEE
                        };

                        newChildTransactions.Add(newChildTransaction);
                    }
                    else if (command.PaymentServiceType == PaymentServiceType.VerificationFee)
                    {
                        var newChildTransaction = new TransactionDto()
                        {
                            FromAccountId = customerCashWalletAccount.Id,
                            ToAccountId = tenantLoanAccount.Id,
                            TenantId = command.TenantId,
                            Amount = command.Amount,
                            TransactionType = TransactionType.VerificationFee,
                            Description = TransactionDescriptionConstants.VERIFICATION_FEE
                        };

                        newChildTransactions.Add(newChildTransaction);
                    }

                    var newTransactions = new List<TransactionDto>()
                    {
                        new()
                        {
                            FromAccountId = customerBankAccount.Id,
                            ToAccountId =  tenantBankAccount.Id,
                            TenantId = command.TenantId,
                            Amount = command.Amount,
                            TransactionType = TransactionType.Transfer,
                            Description = string.Format(TransactionDescriptionConstants.PAYMENT_TO_TENANT_BANK, tenant.Title),
                            Children = [
                                new()
                                {
                                    FromAccountId = tenantBankAccount.Id,
                                    ToAccountId = customerCashWalletAccount.Id,
                                    TenantId = command.TenantId,
                                    TransactionType = TransactionType.Charge,
                                    Description = TransactionDescriptionConstants.CHARGE_CASH_WALLET,
                                    Amount = command.Amount,
                                    Children = newChildTransactions
                                }
                            ]
                        }
                    };

                    var transactions = await _transactionService.CreateTransactionAsync(newTransactions, transactionScope);

                    FinancialDocument financialDocument = null;

                    if (command.PaymentServiceType == PaymentServiceType.OperationFee)
                    {
                        var customerCashWalletToTenantLoanTransaction = transactions
                            .FirstOrDefault(x => x.FromAccountId == customerCashWalletAccount.Id &&
                                x.ToAccountId == tenantLoanAccount.Id && 
                                x.Type == TransactionType.OperationalFee);

                        financialDocument = await CreateFinancialDocument(customerCashWalletToTenantLoanTransaction, 
                            command.Amount, FinancialDocumentType.OperationalFee, command);
                    }
                    else if (command.PaymentServiceType == PaymentServiceType.VerificationFee)
                    {
                        var customerCashWalletToTenantLoanTransaction = transactions
                            .FirstOrDefault(x => x.FromAccountId == customerCashWalletAccount.Id &&
                                x.ToAccountId == tenantLoanAccount.Id &&
                                x.Type == TransactionType.VerificationFee);

                        financialDocument = await CreateFinancialDocument(customerCashWalletToTenantLoanTransaction,
                            command.Amount, FinancialDocumentType.VerificationFee, command);
                    }

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    transactionScope.Complete();

                    return financialDocument.FinancialDocumentPayments.Select(x => new SetTransactionDto
                    {
                        PaymentDetailId = x.PaymentDetailId.Value,
                        TransactionId = x.TransactionId
                    }).ToList();                    
                }
                catch (System.Exception ex)
                {
                    transactionScope.Dispose();
                    throw;
                }
            }
        }

        private async Task ValidateInputData(SetPaymentTransactionCommand command)
        {
            foreach (var paymentDetailDto in command.PaymentDetails)
            {
                paymentDetailDto.WalletId = paymentDetailDto.WalletId == null || paymentDetailDto.WalletId == 0 ? null : paymentDetailDto.WalletId;
                paymentDetailDto.WalletContractId = paymentDetailDto.WalletContractId == null || paymentDetailDto.WalletContractId == 0 ? null : paymentDetailDto.WalletContractId;
            }

            Customer customer = await _customerRepository.GetAsync(command.CustomerId)
                ?? throw new CustomerNotFoundException("مشتری یافت نشد.");
            if (customer.TenantId != command.TenantId)
            {
                throw new ArgumentValidationException(nameof(command.CustomerId), "این مشتری به این مالک زیرساخت تعلق ندارد");
            }
        }

        private async Task<FinancialDocument> CreateFinancialDocument(TransactionAggregate transaction, decimal amount, FinancialDocumentType financialDocumentType, SetPaymentTransactionCommand command)
        {
            List<FinancialDocumentPayment> financialDocumentPayments = [];

            var tenantPlatformContract = await _tenantPlatformContractRepository.GetByTenantIdAsync(command.TenantId) ?? throw new TenantPlatformContractNotFoundException("قرارداد بین پلتفرم و مالک زیرساخت یافت نشد");
            var financialDocument = new FinancialDocument(command.CustomerId, command.TenantId, command.TenantId, amount, command.PaymentId, financialDocumentType, FinancialDocumentState.Verified, null, financialDocumentType.GetEnumDescription(), tenantPlatformContractId: tenantPlatformContract.Id);
            financialDocumentPayments.Add(new FinancialDocumentPayment(command.CustomerId, command.TenantId, FinancialDocumentPaymentType.Cash, amount, command.PaymentDetails.First().WalletId, command.PaymentDetails.First().WalletContractId, transaction, command.PaymentDetails.First().PaymentDetailId));
            financialDocument.SetFinancialDocumentPayments(financialDocumentPayments);
            await _financialDocumentRepository.AddAsync(financialDocument);
            return financialDocument;
        }

        private async Task SetCashWallet(SetPaymentTransactionCommand command)
        {
            var paymentDetail = command.PaymentDetails.FirstOrDefault(x => x.Type != FinancialDocumentPaymentType.Credit && (x.WalletId == null || x.WalletId == 0));

            if (paymentDetail == null)
            {
                return;
            }

            var cashWallet = await _walletRepository.GetCashWalletAsync(command.CustomerId);
            if (cashWallet != null)
            {
                paymentDetail.WalletId = cashWallet?.Id;
                paymentDetail.WalletContractId = cashWallet?.WalletContractId == 0 ? null : cashWallet?.WalletContractId;
            }
        }
    }
}