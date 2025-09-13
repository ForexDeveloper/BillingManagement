using Application.Service.Contracts;
using Application.Service.Dtos.BillingPayments;
using Application.Service.Dtos.Transactions;
using Application.Service.Dtos.Wallet;
using Domain.Core.Entities.AccountAggregate;
using Domain.Core.Entities.BillingAggregate;
using Domain.Core.Entities.BillingAggregate.Exceptions;
using Domain.Core.Entities.CustomerAggregate;
using Domain.Core.Entities.CustomerAggregate.Exceptions;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.FinancialDocumentAggregate.Constants;
using Domain.Core.Entities.InstallmentAggregate;
using Domain.Core.Entities.InstallmentAggregate.Exceptions;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.TenantAggregate.Exceptions;
using Domain.Core.Entities.TenantPlatformContractAggregate;
using Domain.Core.Entities.TenantPlatformContractAggregate.Exceptions;
using Domain.Core.Entities.WalletAggregate;
using Domain.Core.Enums;
using Domain.Core.Helper;
using Domain.Core.UnitOfWorkContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Service.Services;

public class BillingPaymentService : IBillingPaymentService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IFinancialDocumentRepository _financialDocumentRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IBillingRepository _billingRepository;
    private readonly IInstallmentRepository _installmentRepository;
    private readonly ITransactionService _transactionService;
    private readonly IWalletRepository _walletRepository;
    private readonly ITenantPlatformContractRepository _tenantPlatformContractRepository;
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;

    public BillingPaymentService(
        ITenantRepository tenantRepository,
        IFinancialDocumentRepository financialDocumentRepository,
        IAccountRepository accountRepository,
        ICustomerRepository customerRepository,
        IBillingRepository billingRepository,
        IInstallmentRepository installmentRepository,
        ITransactionService transactionService,
        IWalletRepository walletRepository,
        ITenantPlatformContractRepository tenantPlatformContractRepository,
        IApplicationDbContextUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _financialDocumentRepository = financialDocumentRepository;
        _accountRepository = accountRepository;
        _customerRepository = customerRepository;
        _billingRepository = billingRepository;
        _installmentRepository = installmentRepository;
        _transactionService = transactionService;
        _walletRepository = walletRepository;
        _tenantPlatformContractRepository = tenantPlatformContractRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task CreateSettlemetChequePayment(SettlementChequePaymentDto request)
    {
        var existingFinancialDocument = await _financialDocumentRepository.GetAsync(request.PaymentId);
        if (existingFinancialDocument != null)
        {
            throw new ArgumentValidationException(nameof(request.PaymentId), "سند مالی با این شناسه پرداخت وجود دارد.");
        }

        var tenant = await _tenantRepository.GetAsync(request.TenantId) ?? throw new TenantNotFoundException("مالک زیر ساخت یافت نشد");
        var billing = await ValidateAndGetBilling(request);

        var installment = await _installmentRepository.GetById(request.InstallmentId) ?? throw new InstallmentNotFoundException("قسط یافت نشد");
        installment.UpdateStatus(InstallmentState.CompletePaid);
        installment.SetPaidAmount(billing.Amount);

        List<TransactionDto> transactions = await CreateTransactions(request, tenant, billing);

        using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);
        try
        {
            var allTransactions = await _transactionService.CreateTransactionAsync(transactions, transactionScope);
            var customerPaymentTransaction = allTransactions.FirstOrDefault(x => x.Type == TransactionType.Transfer && x.ParentId == null);

            billing.AddBillingPayment(new BillingPayment(billing.Id, DateTime.Now, billing.Amount, customerPaymentTransaction, BillingPaymentState.Paid));
            billing.UpdateState(BillingState.CompletePaid);

            var walletIdentifierDto = await GetCashWallet(request.CustomerId);

            List<FinancialDocumentPayment> financialDocumentPayments = [];
            financialDocumentPayments.Add(new FinancialDocumentPayment(
                request.CustomerId,
                billing.ToBusinessIdentityId,
                FinancialDocumentPaymentType.Cheque,
                billing.Amount,
                walletIdentifierDto.WalletId,
                walletIdentifierDto.WalletContractId,
                customerPaymentTransaction,
                null));

            var financialDocument = await CreateFinancialDocument(financialDocumentPayments, billing.ToBusinessIdentityId, billing.Amount, request);

            await _financialDocumentRepository.AddAsync(financialDocument);
            await _unitOfWork.SaveChangesAsync();
            transactionScope.Complete();
        }
        catch (Exception)
        {
            transactionScope.Dispose();
            throw;
        }
    }

    private async Task<Billing> ValidateAndGetBilling(SettlementChequePaymentDto command)
    {
        var customer = await _customerRepository.GetAsync(command.CustomerId)
            ?? throw new CustomerNotFoundException("مشتری یافت نشد.");

        if (customer.TenantId != command.TenantId)
        {
            throw new ArgumentValidationException(nameof(command.CustomerId), "این مشتری به این مالک زیرساخت تعلق ندارد");
        }

        var billing = await _billingRepository.GetBillingByInstallmentIdAsync(command.InstallmentId, command.TenantId);
        if (billing == null)
        {
            throw new BillingNotFoundException("صورت حساب یافت نشد.");
        }

        if (billing.SettlementType != WalletSettlementType.Cheque)
        {
            throw new ArgumentValidationException(nameof(billing.Id), "صورت حساب به صورت چک تسویه نمی باشد.");
        }

        if (billing.State == BillingState.CompletePaid)
        {
            throw new ArgumentValidationException(nameof(billing.Id), "صورت حساب قبلا پرداخت شده است.");
        }

        if (billing.FromAccount.BusinessIdentityId != command.CustomerId)
        {
            throw new ArgumentValidationException(nameof(command.CustomerId), "صورت حساب مورد نظر به شما تعلق ندارد.");
        }

        return billing;
    }

    private async Task<List<TransactionDto>> CreateTransactions(SettlementChequePaymentDto context, Tenant tenant, Billing billing)
    {
        var businessIdentityIds = new List<int>() { context.CustomerId, context.TenantId };
        var accounts = await _accountRepository.GetByBusinessIdentityIds(businessIdentityIds, context.TenantId);

        var customerBankAccount = accounts.FirstOrDefault(x => x.BusinessIdentityId == context.CustomerId && x.Type == AccountType.Bank);
        var customerCashwalletAccount = accounts.FirstOrDefault(x => x.BusinessIdentityId == context.CustomerId && x.Type == AccountType.CashWallet);
        var tenantBankAccount = accounts.FirstOrDefault(x => x.BusinessIdentityId == context.TenantId && x.Type == AccountType.Bank);
        var tenantLoanAccount = accounts.FirstOrDefault(x => x.BusinessIdentityId == context.TenantId && x.Type == AccountType.Loan);

        var childTransaction = new TransactionDto()
        {
            FromAccountId = customerCashwalletAccount.Id,
            ToAccountId = tenantLoanAccount.Id,
            TenantId = context.TenantId,
            Amount = billing.Amount,
            TransactionType = TransactionType.Charge,
            Description = string.Format(TransactionDescriptionConstants.CHARGE_TO_TENANT_LOAN, tenant.Title)
        };

        var transactions = new List<TransactionDto>()
        {
            new()
            {
                FromAccountId = customerBankAccount.Id,
                ToAccountId = tenantBankAccount.Id,
                TenantId = context.TenantId,
                Amount =  billing.Amount,
                TransactionType = TransactionType.Transfer,
                Description = string.Format(TransactionDescriptionConstants.PAYMENT_TO_TENANT_BANK, tenant.Title),
                Children =
                [
                    new()
                    {
                        FromAccountId = tenantBankAccount.Id,
                        ToAccountId = customerCashwalletAccount.Id,
                        TenantId = context.TenantId,
                        TransactionType = TransactionType.Charge,
                        Description = TransactionDescriptionConstants.CHARGE_CASH_WALLET,
                        Amount = billing.Amount,
                        Children = [childTransaction]
                    }
                ]
            }
        };
        return transactions;
    }

    private async Task<FinancialDocument> CreateFinancialDocument(List<FinancialDocumentPayment> financialDocumentPayments, int toBusinessIdentityId, decimal amount, SettlementChequePaymentDto command)
    {
        var tenantPlatformContract = await _tenantPlatformContractRepository.GetByTenantIdAsync(command.TenantId) ?? throw new TenantPlatformContractNotFoundException("قرارداد بین پلتفرم و مالک زیرساخت یافت نشد");
        var financialDocument = new FinancialDocument(command.CustomerId, toBusinessIdentityId, command.TenantId, amount, command.PaymentId,
            FinancialDocumentType.Billing, FinancialDocumentState.Verified, null, FinancialDocumentType.Billing.GetEnumDescription(), tenantPlatformContractId: tenantPlatformContract.Id);
        financialDocument.SetFinancialDocumentPayments(financialDocumentPayments);

        return financialDocument;
    }

    private async Task<WalletIdentifierDto> GetCashWallet(int customerId)
    {
        int? walletId = null;
        int? walletContractId = null;

        var cashWallet = await _walletRepository.GetCashWalletAsync(customerId);
        if (cashWallet != null)
        {
            walletId = cashWallet?.Id;
            walletContractId = cashWallet?.WalletContractId == 0 ? null : cashWallet?.WalletContractId;
        }

        return new WalletIdentifierDto
        {
            WalletId = walletId,
            WalletContractId = walletContractId
        };
    }
}
