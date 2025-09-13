using Application.Service.Contracts;
using Application.Service.Dtos.CashOut;
using Application.Service.Dtos.CashWallet;
using Application.Service.Dtos.Transactions;
using Domain.Core.Entities;
using Domain.Core.Entities.AccountAggregate;
using Domain.Core.Entities.AccountAggregate.Exceptions;
using Domain.Core.Entities.BankAccountAggregate;
using Domain.Core.Entities.BankAccountAggregate.Exceptions;
using Domain.Core.Entities.CashOutRequestAggregate;
using Domain.Core.Entities.CustomerAggregate;
using Domain.Core.Entities.CustomerAggregate.Exceptions;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.FinancialDocumentAggregate.Constants;
using Domain.Core.Entities.FinancialDocumentAggregate.Exceptions;
using Domain.Core.Entities.TenantPlatformContractAggregate;
using Domain.Core.Entities.TenantPlatformContractAggregate.Exceptions;
using Domain.Core.Entities.TransactionAggregate;
using Domain.Core.Entities.WalletAggregate;
using Domain.Core.Enums;
using Domain.Core.UnitOfWorkContracts;
using Shared.EventBus.Contracts;
using Shared.EventBus.Enums;
using Shared.EventBus.Events;
using Shared.IdentityServerProvider;
using Shared.IdentityServerProvider.Contracts;
using Shared.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Service.Services;

public class CashOutRequestService : ICashOutRequestService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICashOutRequestRepository _cashOutRequestRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly ITransactionService _transactionService;
    private readonly ITenantPlatformContractRepository _tenantPlatformContractRepository;
    private readonly IFinancialDocumentRepository _financialDocumentRepository;
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;
    private readonly ICustomerRepository _customerRepository;
    private readonly IOutboxService _outboxService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBankAccountRepository _bankAccountRepository;

    public CashOutRequestService(IAccountRepository accountRepository,
        ICashOutRequestRepository cashOutRequestRepository,
        ITransactionRepository transactionRepository, IWalletRepository walletRepository, ITransactionService transactionService,
        ITenantPlatformContractRepository tenantPlatformContractRepository,
        IFinancialDocumentRepository financialDocumentRepository, IApplicationDbContextUnitOfWork unitOfWork,
        ICustomerRepository customerRepository, IOutboxService outboxService, ICurrentUserService currentUserService,
        IBankAccountRepository bankAccountRepository)
    {
        _accountRepository = accountRepository;
        _cashOutRequestRepository = cashOutRequestRepository;
        _transactionRepository = transactionRepository;
        _walletRepository = walletRepository;
        _transactionService = transactionService;
        _tenantPlatformContractRepository = tenantPlatformContractRepository;
        _financialDocumentRepository = financialDocumentRepository;
        _unitOfWork = unitOfWork;
        _customerRepository = customerRepository;
        _outboxService = outboxService;
        _currentUserService = currentUserService;
        _bankAccountRepository = bankAccountRepository;
    }

    public async Task<CashOutResult> CreateCashOutRequest(CreateCashOutRequestDto request, CancellationToken cancellationToken)
    {
        var businessIdentityIds = new List<int>() { request.BusinessIdentityId, request.TenantId };

        var accounts = await _accountRepository.GetByBusinessIdentityIds(businessIdentityIds, request.TenantId);

        var customerCashWalletAccount = accounts.FirstOrDefault(x => x.BusinessIdentityId == request.BusinessIdentityId
            && x.Type == AccountType.CashWallet) ?? throw new AccountNotFoundException("حساب کیف پول نقدی مشتری یافت نشد.");

        if (customerCashWalletAccount.TenantId != request.TenantId)
            throw new ArgumentValidationException("CustomerId", "این مشتری به این مالک زیرساخت تعلق ندارد");

        if (customerCashWalletAccount.WithDrawableBalance < request.Amount)
            throw new ArgumentValidationException(nameof(request.Amount), "مبلغ درخواست شده بیشتر از موجودی کیف پول نقدی می باشد.");

        var tenantBankAccount = accounts.FirstOrDefault(x => x.BusinessIdentityId == request.TenantId
            && x.Type == AccountType.Bank);

        var newTransactions = new List<TransactionDto>()
            {
                new()
                {
                    FromAccountId = customerCashWalletAccount.Id,
                    ToAccountId =  tenantBankAccount.Id,
                    TenantId = request.TenantId,
                    Amount = request.Amount,
                    TransactionType = TransactionType.Withdrawal,
                    Description = TransactionDescriptionConstants.WITHDRAW_CASH_WALLET,
                }
            };

        using (TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                var wallet = await _walletRepository.GetCashWalletAsync(request.BusinessIdentityId) ?? throw new ArgumentValidationException("CustomerId", "کیف پول نقدی مشتری پیدا نشد.");

                var transactions = await _transactionService.CreateTransactionAsync(newTransactions, transactionScope);
                var transaction = transactions.FirstOrDefault();

                List<FinancialDocumentPayment> financialDocumentPayments = [];
                financialDocumentPayments.Add(new FinancialDocumentPayment(
                        request.BusinessIdentityId,
                        request.TenantId,
                        FinancialDocumentPaymentType.Cash,
                        request.Amount,
                        wallet.Id,
                        null,
                        transaction,
                        null));

                var financialDocument = await CreateFinancialDocument(financialDocumentPayments, request.TenantId, request.BusinessIdentityId, request.TenantId, request.Amount, FinancialDocumentType.WithdrawCashOut);

                await _financialDocumentRepository.AddAsync(financialDocument);
                var cashOutRequest = new CashOutRequest(wallet.Id, financialDocument, request.BankAccountId, request.TenantId);
                await _cashOutRequestRepository.AddAsync(cashOutRequest);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                transactionScope.Complete();

                return new CashOutResult(cashOutRequest.Id, transaction.Id);
            }
            catch (Exception)
            {
                transactionScope.Dispose();
                throw;
            }
        }
    }

    public async Task AproveCustomerCashOutRequest(CashOutRequest cashOutRequest, string bankTransactionCode, string description, Customer customer, CancellationToken cancellationToken)
    {
        using (TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                cashOutRequest.Approve(bankTransactionCode, description);

                var financialDocument = cashOutRequest.FinancialDocument ?? throw new FinancialDocumentNotFoundException("سند مالی پیدا نشد.");
                var financialDocumentPayment = financialDocument.FinancialDocumentPayments.FirstOrDefault(x => x.FinancialDocumentId == financialDocument.Id && x.Type == FinancialDocumentPaymentType.Cash) ?? throw new FinancialDocumentPaymentNotFoundException("سند مالی پرداخت پیدا نشد.");

                var businessIdentityIds = new List<int>() { financialDocument.FromBusinessIdentityId,
                    cashOutRequest.TenantId };

                var accounts = await _accountRepository.GetByBusinessIdentityIds(businessIdentityIds, cashOutRequest.TenantId);

                var tenantBankAccount = accounts.FirstOrDefault(x => x.BusinessIdentityId == cashOutRequest.TenantId
                    && x.Type == AccountType.Bank) ?? throw new AccountNotFoundException("حساب بانک مالک زیر ساخت یافت نشد.");

                var customerBankAccount = accounts.FirstOrDefault(x => x.BusinessIdentityId == financialDocument.FromBusinessIdentityId
                    && x.Type == AccountType.Bank) ?? throw new AccountNotFoundException("حساب بانک مشتری یافت نشد.");

                var newTransactions = new List<TransactionDto>()
                {
                    new()
                    {
                        FromAccountId = tenantBankAccount.Id,
                        ToAccountId = customerBankAccount.Id,
                        TenantId = cashOutRequest.TenantId,
                        Amount = financialDocument.Amount,
                        TransactionType = TransactionType.Charge,
                        Description = TransactionDescriptionConstants.PAYMENT_TO_CUSTOMER_BANK,
                        ParentId = financialDocumentPayment.TransactionId
                    }
                };

                var transactions = await _transactionService.CreateTransactionAsync(newTransactions, transactionScope);

                SendApprovedOrRejectedSms(customer.FullName, customer.Mobile, _currentUserService.UserId ?? string.Empty, cashOutRequest.TenantId, NotificationTemplateActionType.CustomerCashOutRequestApproved, cashOutRequest.FollowUpCode);

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

    public async Task RejectCustomerCashOutRequest(CashOutRequest cashOutRequest, CashOutRequestRejectReason reason, string description, Customer customer, CancellationToken cancellationToken)
    {
        cashOutRequest.Reject(reason, description);

        var financialDocument = cashOutRequest.FinancialDocument ?? throw new FinancialDocumentNotFoundException("سند مالی پیدا نشد.");
        var financialDocumentPayment = financialDocument.FinancialDocumentPayments.FirstOrDefault(x => x.FinancialDocumentId == financialDocument.Id && x.Type == FinancialDocumentPaymentType.Cash) ?? throw new FinancialDocumentPaymentNotFoundException("سند مالی پرداخت پیدا نشد.");

        var businessIdentityIds = new List<int>() { financialDocument.FromBusinessIdentityId,
                    financialDocument.ToBusinessIdentityId };

        var accounts = await _accountRepository.GetByBusinessIdentityIds(businessIdentityIds, cashOutRequest.TenantId);

        var tenantBankAccount = accounts.FirstOrDefault(x => x.BusinessIdentityId == financialDocument.ToBusinessIdentityId
            && x.Type == AccountType.Bank) ?? throw new AccountNotFoundException("حساب بانک مالک زیر ساخت یافت نشد.");

        var customerCashWalletAccount = accounts.FirstOrDefault(x => x.BusinessIdentityId == financialDocument.FromBusinessIdentityId
            && x.Type == AccountType.CashWallet) ?? throw new AccountNotFoundException("حساب کیف پول نقدی مشتری یافت نشد.");

        var newTransactions = new List<TransactionDto>()
                {
                    new()
                    {
                        FromAccountId = tenantBankAccount.Id,
                        ToAccountId = customerCashWalletAccount.Id,
                        TenantId = cashOutRequest.TenantId,
                        Amount = financialDocument.Amount,
                        TransactionType = TransactionType.Charge,
                        Description = TransactionDescriptionConstants.CHARGE_CASH_WALLET,
                        ParentId = financialDocumentPayment.TransactionId
                    }
                };

        using (TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                var transactions = await _transactionService.CreateTransactionAsync(newTransactions, transactionScope);
                var transaction = transactions.FirstOrDefault();

                List<FinancialDocumentPayment> financialDocumentPayments = [];
                financialDocumentPayments.Add(new FinancialDocumentPayment(
                        financialDocument.ToBusinessIdentityId,
                        financialDocument.FromBusinessIdentityId,
                        FinancialDocumentPaymentType.Cash,
                        financialDocument.Amount,
                        financialDocumentPayment.WalletId,
                        null,
                        transaction,
                        null));

                var newFinancialDocument = await CreateFinancialDocument(financialDocumentPayments, cashOutRequest.TenantId,
                    financialDocument.ToBusinessIdentityId, financialDocument.FromBusinessIdentityId, financialDocument.Amount, FinancialDocumentType.WalletCharge);
                newFinancialDocument.SetParentId(financialDocument.Id);

                await _financialDocumentRepository.AddAsync(newFinancialDocument);
                SendApprovedOrRejectedSms(customer.FullName, customer.Mobile, _currentUserService.UserId ?? string.Empty, cashOutRequest.TenantId, NotificationTemplateActionType.CustomerCashOutRequestRejected);
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

    private async Task<FinancialDocument> CreateFinancialDocument(List<FinancialDocumentPayment> financialDocumentPayments, int tenantId, int fromBusinessIdentityId, int toBusinessIdentityId, decimal amount, FinancialDocumentType financialDocumentType)
    {
        var tenantPlatformContract = await _tenantPlatformContractRepository.GetByTenantIdAsync(tenantId) ?? throw new TenantPlatformContractNotFoundException("قرارداد بین پلتفرم و مالک زیرساخت یافت نشد");
        var financialDocument = new FinancialDocument(
            fromBusinessIdentityId, toBusinessIdentityId, tenantId, amount, null,
            financialDocumentType, FinancialDocumentState.Verified, null, financialDocumentType.GetEnumDescription(),
            tenantPlatformContractId: tenantPlatformContract.Id);

        financialDocument.SetFinancialDocumentPayments(financialDocumentPayments);

        return financialDocument;
    }

    public async Task<Customer> ValidateCustomer(int customerId, int tenantId)
    {
        var customer = await _customerRepository.GetAsync(customerId);

        if (customer == null || customer.TenantId != tenantId)
            throw new CustomerNotFoundException("مشتری یافت نشد.");

        return customer;
    }

    public async Task ValidateBankAccount(int bankAccountId, Customer customer)
    {
        var bankAccount = await _bankAccountRepository.GetByIdAsync(bankAccountId, customer.TenantId);

        if (bankAccount == null || bankAccount.BusinessIdentityId != customer.Id)
            throw new BankAccountNotFoundException("حساب بانکی پیدا نشد.");
    }

    public async Task<CashOutRequest> GetCashOutRequest(long cashOutRequestId, int customerId, int tenantId)
    {
        var cashOutRequest = await _cashOutRequestRepository.GetByIdAsync(cashOutRequestId) ?? throw new CashOutRequestNotFoundException("درخواست برداشت وجه پیدا نشد.");

        if (cashOutRequest.CashWallet.BusinessIdentityId != customerId)
            throw new ArgumentValidationException(nameof(customerId), "شناسه مشتری معتبر نمی باشد.");

        if (cashOutRequest.TenantId != tenantId)
            throw new TenantForbiddenException();

        return cashOutRequest;
    }

    public void SendApprovedOrRejectedSms(string fullName, string mobile, string userId, int tenantId, NotificationTemplateActionType notificationTemplateActionType, long? followUpCode = null)
    {
        var keyValues = new List<KeyValuePair<string, string>>()
        {
            new("FullName", fullName),
        };

        if (followUpCode != null)
        {
            keyValues.Add(new KeyValuePair<string, string>("FollowUpCode", followUpCode.ToString()));
        }

        _outboxService.AddNewEvent(new NotifSmsRequestAddedEvent()
        {
            TenantId = tenantId,
            KeyValues = keyValues,
            ActionType = notificationTemplateActionType,
            MobileNumber = mobile,
            MaxTryCount = 3,
            DueDateTime = DateTime.Now,
            UserId = userId
        });
    }
}