using Application.Command.TransactionCommands.Dtos;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Service.Contracts;
using Application.Service.Dtos.FinancialDocuments;
using Application.Service.Dtos.Transactions;
using Application.Service.Dtos.Wallet;
using Application.Service.Helper;
using Domain.Core.Entities.AccountAggregate;
using Domain.Core.Entities.CustomerAggregate;
using Domain.Core.Entities.CustomerAggregate.Exceptions;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.FinancialDocumentAggregate.Constants;
using Domain.Core.Entities.InstallmentAggregate;
using Domain.Core.Entities.MerchantAggregate;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.TenantAggregate.Exceptions;
using Domain.Core.Entities.TenantMerchantContractAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate.Exceptions;
using Domain.Core.Entities.TenantPlatformContractAggregate;
using Domain.Core.Entities.TenantPlatformContractAggregate.Exceptions;
using Domain.Core.Entities.WalletAggregate;
using Domain.Core.Entities.WalletAggregate.Exceptions;
using Domain.Core.Entities.WalletContractsAggregate.Exceptions;
using Domain.Core.Enums;
using Domain.Core.UnitOfWorkContracts;
using MediatR;
using Shared.EventBus.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Domain.Core.Entities.MerchantAggregate.Exceptions;
using Domain.Core.Entities.MerchantBillingAggregate;
using Domain.Core.Entities.MerchantInstallmentAggregate;

namespace Application.Command.TransactionCommands;
public class ValidateAndSetTransactionCommand : IRequest<List<SetTransactionDto>>
{
    public ValidateAndSetTransactionCommand(int tenantId, int fromBusinessIdentityId, int toBusinessIdentityId, int? merchantBranchId, decimal amount, long paymentId, PurchaseGatewayType purchaseGatewayType, List<PaymentDetailDto> payments)
    {
        TenantId = tenantId;
        FromBusinessIdentityId = fromBusinessIdentityId;
        ToBusinessIdentityId = toBusinessIdentityId;
        MerchantBranchId = merchantBranchId;
        Amount = amount;
        PaymentId = paymentId;
        PurchaseGatewayType = purchaseGatewayType;
        PaymentDetails = payments;
    }

    public int TenantId { get; set; }
    public int FromBusinessIdentityId { get; private set; }
    public int ToBusinessIdentityId { get; private set; }
    public int? MerchantBranchId { get; private set; }
    public decimal Amount { get; private set; }
    public long PaymentId { get; private set; }
    public PurchaseGatewayType PurchaseGatewayType { get; private set; }
    public List<PaymentDetailDto> PaymentDetails { get; set; }

    public class ValidateAndSetTransactionCommandHandler : IRequestHandler<ValidateAndSetTransactionCommand, List<SetTransactionDto>>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly ITenantMerchantContractRepository _tenantMerchantContractRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly IFinancialDocumentRepository _financialDocumentRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IMerchantRepository _merchantRepository;
        private readonly IApplicationDbContextUnitOfWork _unitOfWork;
        private readonly IWalletReadOnlyRepository _walletReadOnlyRepository;
        private readonly ITransactionService _transactionService;
        private readonly ITenantPlatformContractRepository _tenantPlatformContractRepository;
        private readonly IMerchantInstallmentRepository _merchantInstallmentRepository;

        public ValidateAndSetTransactionCommandHandler(
            ITenantRepository tenantRepository,
            IFinancialDocumentRepository financialDocumentRepository,
            IApplicationDbContextUnitOfWork unitOfWork,
            IAccountRepository accountRepository,
            IWalletRepository walletRepository,
            ITenantMerchantContractRepository tenantMerchantContractRepository,
            ICustomerRepository customerRepository,
            IMerchantRepository merchantRepository,
            IWalletReadOnlyRepository walletReadOnlyRepository, ITransactionService transactionService,
            ITenantPlatformContractRepository tenantPlatformContractRepository, IMerchantInstallmentRepository merchantInstallmentRepository)
        {
            _tenantRepository = tenantRepository;
            _financialDocumentRepository = financialDocumentRepository;
            _unitOfWork = unitOfWork;
            _accountRepository = accountRepository;
            _walletRepository = walletRepository;
            _tenantMerchantContractRepository = tenantMerchantContractRepository;
            _customerRepository = customerRepository;
            _merchantRepository = merchantRepository;
            _walletReadOnlyRepository = walletReadOnlyRepository;
            _transactionService = transactionService;
            _tenantPlatformContractRepository = tenantPlatformContractRepository;
            _merchantInstallmentRepository = merchantInstallmentRepository;
        }

        public async Task<List<SetTransactionDto>> Handle(ValidateAndSetTransactionCommand command, CancellationToken cancellationToken)
        {
            List<SetTransactionDto> setTransactionDtos = [];
            await ValidateInputData(command);

            var tenant = await _tenantRepository.GetAsync(command.TenantId) ?? throw new TenantNotFoundException("مالک زیر ساخت یافت نشد");

            List<FinancialDocumentPayment> financialDocumentPayments = [];
            var newTransactions = new List<TransactionDto>();
            bool hasCustomerChargeTransaction = false;

            using (TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var businessIdentityIds = new List<int>() { command.FromBusinessIdentityId, command.TenantId };
                    var accounts = await _accountRepository.GetByBusinessIdentityIds(businessIdentityIds, command.TenantId);

                    var customerBankAccount = accounts.FirstOrDefault(x => x.BusinessIdentityId == command.FromBusinessIdentityId
                        && x.Type == AccountType.Bank);

                    var customerCashWalletAccount = accounts.FirstOrDefault(x => x.BusinessIdentityId == command.FromBusinessIdentityId
                        && x.Type == AccountType.CashWallet);

                    var tenantPurchaseAccount = accounts.FirstOrDefault(x => x.BusinessIdentityId == command.TenantId
                            && x.Type == AccountType.Purchase);

                    var tenantBankAccount = accounts.FirstOrDefault(x => x.BusinessIdentityId == command.TenantId
                            && x.Type == AccountType.Bank);

                    foreach (var paymentDto in command.PaymentDetails.OrderBy(x => x.Type))
                    {
                        if (paymentDto.Type == FinancialDocumentPaymentType.Credit)
                        {
                            var wallet = await _walletRepository.GetByIdAsync(paymentDto.WalletId.Value);

                            var newTransaction = new TransactionDto
                            {
                                FromAccountId = wallet.AccountId,
                                ToAccountId = tenantPurchaseAccount.Id,
                                TenantId = command.TenantId,
                                Amount = paymentDto.Amount,
                                TransactionType = TransactionType.Purchase,
                                Description = string.Format(TransactionDescriptionConstants.TRANSFER_TO_TENANT_PURCHASE, tenant.Title)
                            };

                            newTransactions.Add(newTransaction);
                        }
                        else if (paymentDto.Type == FinancialDocumentPaymentType.Cash || paymentDto.Type == FinancialDocumentPaymentType.Prepayment)
                        {
                            if (!hasCustomerChargeTransaction)
                            {
                                var cashAndPrepaymentAmount = command.PaymentDetails.Where(x => x.Type == FinancialDocumentPaymentType.Prepayment || x.Type == FinancialDocumentPaymentType.Cash).Sum(x => x.Amount);

                                var newTransaction = new TransactionDto()
                                {
                                    FromAccountId = customerBankAccount.Id,
                                    ToAccountId = tenantBankAccount.Id,
                                    TenantId = command.TenantId,
                                    Amount = cashAndPrepaymentAmount,
                                    TransactionType = TransactionType.Transfer,
                                    Description = string.Format(TransactionDescriptionConstants.PAYMENT_TO_TENANT_BANK, tenant.Title),
                                    Children = [
                                     new()
                                                {
                                                    FromAccountId = tenantBankAccount.Id,
                                                    ToAccountId = customerCashWalletAccount.Id,
                                                    TenantId = command.TenantId,
                                                    TransactionType = TransactionType.Charge,
                                                    Description =  TransactionDescriptionConstants.CHARGE_CASH_WALLET,
                                                    Amount = cashAndPrepaymentAmount,
                                                    Children = [
                                                        new()
                                                        {
                                                            FromAccountId = customerCashWalletAccount.Id,
                                                            ToAccountId = tenantPurchaseAccount.Id,
                                                            TenantId = command.TenantId,
                                                            TransactionType = TransactionType.Purchase,
                                                            Description =  string.Format(TransactionDescriptionConstants.CHARGE_TO_TENANT_PURCHASE, tenant.Title),
                                                            Amount = cashAndPrepaymentAmount
                                                        }
                                                    ]
                                                }
                                     ]
                                };

                                newTransactions.Add(newTransaction);
                                hasCustomerChargeTransaction = true;
                            }
                        }
                    }

                    var transactions = await _transactionService.CreateTransactionAsync(newTransactions, transactionScope);

                    var customerChargeTransaction = transactions
                            .FirstOrDefault(x => x.FromAccountId == customerCashWalletAccount.Id &&
                                x.ToAccountId == tenantPurchaseAccount.Id &&
                                x.Type == TransactionType.Purchase);

                    foreach (var paymentDto in command.PaymentDetails.OrderBy(x => x.Type))
                    {
                        if (paymentDto.Type == FinancialDocumentPaymentType.Credit)
                        {
                            var wallet = await _walletRepository.GetByIdAsync(paymentDto.WalletId.Value);

                            var customerTransferTransaction = transactions
                                .FirstOrDefault(x => x.FromAccountId == wallet.AccountId &&
                                    x.ToAccountId == tenantPurchaseAccount.Id &&
                                    x.Type == TransactionType.Purchase);

                            financialDocumentPayments.Add(new FinancialDocumentPayment(command.FromBusinessIdentityId, command.TenantId, FinancialDocumentPaymentType.Credit, paymentDto.Amount, paymentDto.WalletId, paymentDto.WalletContractId, customerTransferTransaction, paymentDto.PaymentDetailId));
                        }
                        else if (paymentDto.Type == FinancialDocumentPaymentType.Cash)
                        {
                            financialDocumentPayments.Add(new FinancialDocumentPayment(command.FromBusinessIdentityId, command.TenantId, FinancialDocumentPaymentType.Cash, paymentDto.Amount, paymentDto.WalletId, paymentDto.WalletContractId, customerChargeTransaction, paymentDto.PaymentDetailId));
                        }
                        else if (paymentDto.Type == FinancialDocumentPaymentType.Prepayment)
                        {
                            financialDocumentPayments.Add(new FinancialDocumentPayment(command.FromBusinessIdentityId, command.ToBusinessIdentityId, FinancialDocumentPaymentType.Prepayment, paymentDto.Amount, paymentDto.WalletId, paymentDto.WalletContractId, customerChargeTransaction, paymentDto.PaymentDetailId));
                        }
                    }

                    var financialDocument = await CreateFinancialDocument(financialDocumentPayments, command);

                    await _financialDocumentRepository.AddAsync(financialDocument);

                    //var installments = await CreateMerchantInstallments(financialDocument, command);

                    //await _merchantInstallmentRepository.AddRangeAsync(installments);

                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    transactionScope.Complete();

                    setTransactionDtos = financialDocumentPayments.Select(x => new SetTransactionDto
                    {
                        PaymentDetailId = x.PaymentDetailId.Value,
                        TransactionId = x.TransactionId
                    }).ToList();

                    return setTransactionDtos.GroupBy(x => x.PaymentDetailId).Select(x => x.First()).ToList();
                }
                catch (Exception)
                {
                    transactionScope.Dispose();

                    throw;
                }
            }
        }

        private async Task ValidateInputData(ValidateAndSetTransactionCommand command)
        {
            var existingFinancialDocument = await _financialDocumentRepository.GetAsync(command.PaymentId);

            if (existingFinancialDocument != null)
            {
                throw new FinancialDocumentAlreadyExistsException("سند خرید با این شناسه پرداخت وجود دارد");
            }

            var customer = await _customerRepository.GetAsync(command.FromBusinessIdentityId)
                ?? throw new CustomerNotFoundException("مشتری یافت نشد.");

            if (customer.TenantId != command.TenantId)
            {
                throw new ArgumentValidationException(nameof(command.FromBusinessIdentityId), "این مشتری به این مالک زیرساخت تعلق ندارد");
            }

            var merchant = await _merchantRepository.GetAsync(command.ToBusinessIdentityId)
                ?? throw new CustomerNotFoundException("پذیرنده یافت نشد.");

            if (merchant.TenantId != command.TenantId)
            {
                throw new ArgumentValidationException($"{nameof(Merchant)}{nameof(merchant.Id)}", "این پذیرنده به این مالک زیرساخت تعلق ندارد");
            }

            var contract = await _tenantMerchantContractRepository.GetActiveContractAsync(merchant.Id, command.TenantId);

            if (contract == null)
            {
                throw new TenantMerchantContractNotFoundException("قرار داد پذیرنده یافت نشد");
            }

            var creditPayment = command.PaymentDetails.FirstOrDefault(x => x.Type == FinancialDocumentPaymentType.Credit);
            var cashPayment = command.PaymentDetails.FirstOrDefault(x => x.Type == FinancialDocumentPaymentType.Cash);

            if (creditPayment != null)
            {
                var loanWallet = await _walletRepository.GetByIdAsync(creditPayment.WalletId.Value)
                     ?? throw new WalletNotFoundException("کیف پول اعتباری یافت نشد.");
                if (loanWallet.Status == WalletStatus.Deactive)
                {
                    throw new ArgumentValidationException(nameof(loanWallet.Status), "کیف پول اعتباری غیرفعال می باشد");
                }
                if (loanWallet.Status == WalletStatus.Suspend)
                {
                    throw new ArgumentValidationException(nameof(loanWallet.Status), "کیف پول اعتباری معلق می باشد");
                }
                if (loanWallet.WalletContractId == 0)
                {
                    throw new ArgumentValidationException(nameof(loanWallet.WalletContractId), "قراردادی برای این کیف پول اعتباری وجود ندارد");
                }

                creditPayment.WalletContractId = loanWallet.WalletContractId;

                var canUseWallet = loanWallet.Plan.PlanClosedloops
                    .Any(p => p.ClosedLoop.ClosedloopMerchants.Any(m => m.MerchantId == command.ToBusinessIdentityId));
                if (!canUseWallet)
                {
                    throw new ArgumentValidationException(nameof(command.ToBusinessIdentityId), "امکان خرید با این کیف پول اعتباری ازین پذیرنده وجود ندارد");
                }

                var account = await _accountRepository.GetByIdAsync(loanWallet.AccountId);

                if (account.BusinessIdentityId != command.FromBusinessIdentityId)
                {
                    throw new ArgumentValidationException(nameof(account.BusinessIdentityId), "این کیف پول برای این مشتری نمی باشد");
                }

                if (account.Status != AccountStatus.Active)
                {
                    throw new ArgumentValidationException(nameof(loanWallet.Status), "اکانت مشتری غیرفعال می باشد");
                }

                if (account.Balance < creditPayment.Amount)
                {
                    throw new ArgumentValidationException(nameof(account.Balance), "موجودی حساب کمتر از مبلغ درخواستی می باشد");
                }

                decimal prepaymentAmount = await CalculatePrepaymentAmount(loanWallet, command);

                if (cashPayment is null && prepaymentAmount > 0)
                {
                    throw new ArgumentValidationException(nameof(prepaymentAmount), "مبلغ پیش پرداخت کافی نمی باشد");
                }

                if (cashPayment != null)
                {
                    if (prepaymentAmount > cashPayment.Amount)
                    {
                        throw new ArgumentValidationException(nameof(prepaymentAmount), "مبلغ پیش پرداخت کافی نمی باشد");
                    }

                    SetCashAndPrepayments(cashPayment, prepaymentAmount, command);
                }
            }

            await SetCashWallets(command);

            if (command.PaymentDetails.Sum(x => x.Amount) != command.Amount)
            {
                throw new ArgumentValidationException(nameof(command.Amount), "مبلغ کل جزییات پرداخت با مبلغ اصلی برابر نمی باشد.");
            }
        }

        public async Task<decimal> CalculatePrepaymentAmount(Wallet loanWallet, ValidateAndSetTransactionCommand command)
        {
            var prepaymentAmount = await _walletReadOnlyRepository.CalculatePrePaymentAmount(loanWallet.Id, command.Amount);
            return prepaymentAmount;
        }

        private void SetCashAndPrepayments(PaymentDetailDto cashPayment, decimal prepaymentAmount, ValidateAndSetTransactionCommand command)
        {
            if (prepaymentAmount > 0)
            {
                command.PaymentDetails.Add(new PaymentDetailDto
                {
                    Type = FinancialDocumentPaymentType.Prepayment,
                    Amount = prepaymentAmount,
                    PaymentDetailId = cashPayment.PaymentDetailId,
                });
            }

            cashPayment.Amount = cashPayment.Amount - prepaymentAmount;
            if (cashPayment.Amount == 0)
            {
                command.PaymentDetails.Remove(cashPayment);
            }
        }

        private async Task SetCashWallets(ValidateAndSetTransactionCommand command)
        {
            var cashAndPrepaymentDetails = command.PaymentDetails.Where(x => x.Type == FinancialDocumentPaymentType.Cash || x.Type == FinancialDocumentPaymentType.Prepayment);
            if (!cashAndPrepaymentDetails.Any())
            {
                return;
            }

            Wallet? cashWallet = await _walletRepository.GetCashWalletAsync(command.FromBusinessIdentityId);

            foreach (var paymentDetail in command.PaymentDetails.Where(x => x.Type != FinancialDocumentPaymentType.Credit))
            {
                paymentDetail.WalletId = cashWallet?.Id;
                paymentDetail.WalletContractId = cashWallet?.WalletContractId == 0 ? null : cashWallet?.WalletContractId;
            }
        }

        private async Task<FinancialDocument> CreateFinancialDocument(List<FinancialDocumentPayment> financialDocumentPayments, ValidateAndSetTransactionCommand command)
        {
            var tenantMerchantContract = await _tenantMerchantContractRepository.GetActiveContractAsync(command.TenantId, command.ToBusinessIdentityId) ?? throw new TenantMerchantContractNotFoundException("قرارداد بین مالک زیرساخت و پذیرنده یافت نشد");
            var tenantPlatformContract = await _tenantPlatformContractRepository.GetByTenantIdAsync(command.TenantId) ?? throw new TenantPlatformContractNotFoundException("قرارداد بین پلتفرم و مالک زیرساخت یافت نشد");

            var merchant = await _merchantRepository.GetAsync(command.ToBusinessIdentityId);

            PaymentGatewayType paymentGatewayType = PaymentGatewayType.Cpg;
            var purchaseGatewayType = command.PurchaseGatewayType;

            if (purchaseGatewayType == PurchaseGatewayType.Opg)
            {
                paymentGatewayType = PaymentGatewayType.Opg;
            }

            var financialDocument = new FinancialDocument(command.FromBusinessIdentityId, command.ToBusinessIdentityId,
                command.TenantId, command.Amount, command.PaymentId, FinancialDocumentType.Purchase,
                FinancialDocumentState.Verified, paymentGatewayType, $"خرید از {merchant.Title}", command.MerchantBranchId, tenantMerchantContract.Id, tenantPlatformContract.Id);
            financialDocument.SetFinancialDocumentPayments(financialDocumentPayments);
            return financialDocument;
        }

        private async Task<List<MerchantInstallment>> CreateMerchantInstallments(FinancialDocument financialDocument, ValidateAndSetTransactionCommand command)
        {
            List<MerchantInstallment> installments = [];

            var contract = await _tenantMerchantContractRepository.GetActiveContractAsync(command.ToBusinessIdentityId, command.TenantId);

            var installmentDates = DateHelper.CalculateMerchantInstallments(DateTime.Now, contract.InstallmentsCount,
                TimeInterval.Day, contract.BillingBreak, contract.BillingPeriod, contract.BillingPeriodType);

            var installmentCount = contract.InstallmentsCount ?? 1;

            var installmentAmount = RoundHelper.RoundAmount(command.Amount / installmentCount);

            var lastInstallmentAmount = command.Amount - installmentAmount * (installmentCount - 1);
            
            for (var i = 0; i < installmentDates.Count; i++)
            {
                var installmentDate = installmentDates[i];

                var amount = i == installmentDates.Count - 1 ? lastInstallmentAmount : installmentAmount;

                var installment = new MerchantInstallment(financialDocument, command.TenantId,
                    command.TenantId, command.ToBusinessIdentityId, contract.Id, amount, i + 1,
                    installmentDate, B2bInstallmentType.Installment);

                installments.Add(installment);
            }

            return installments;
        }
    }
}