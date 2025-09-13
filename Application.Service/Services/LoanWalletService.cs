using Application.Service.Contracts;
using Application.Service.Dtos.Transactions;
using Application.Service.Dtos.Wallet;
using Application.Service.Helper;
using Domain.Core.Entities.AccountAggregate;
using Domain.Core.Entities.BillingAggregate;
using Domain.Core.Entities.CustomerAggregate;
using Domain.Core.Entities.FinancialDocumentAggregate.Constants;
using Domain.Core.Entities.InstallmentAggregate;
using Domain.Core.Entities.PlanAggregate;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.WalletAggregate;
using Domain.Core.Entities.WalletContractAggregate;
using Domain.Core.Enums;
using Domain.Core.UnitOfWorkContracts;
using Microsoft.Extensions.Logging;
using RedLockNet;
using Shared.EventBus.Contracts;
using Shared.EventBus.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using TransactionAggregate = Domain.Core.Entities.TransactionAggregate.Transaction;

namespace Application.Service.Services;

public class LoanWalletService : ILoanWalletService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IWalletContractRepository _walletContractRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IInstallmentRepository _installmentRepository;
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;
    public readonly ILogger<LoanWalletService> _logger;
    private readonly IOutboxService _outboxService;
    public readonly IBillingRepository _billingRepository;
    public readonly ITransactionService _transactionService;
    private readonly IDistributedLockFactory _distributedLockFactory;
    private readonly IPlanRepository _planRepository;

    public LoanWalletService(ITenantRepository tenantRepository, IAccountRepository accountRepository,
        ICustomerRepository customerRepository,
        IWalletContractRepository walletContractRepository,
        IWalletRepository walletRepository, IInstallmentRepository installmentRepository,
        IApplicationDbContextUnitOfWork unitOfWork, ILogger<LoanWalletService> logger, IOutboxService outboxService,
        IBillingRepository billingRepository, ITransactionService transactionService, IPlanRepository planRepository,
        IDistributedLockFactory distributedLockFactory)
    {
        _tenantRepository = tenantRepository;
        _accountRepository = accountRepository;
        _customerRepository = customerRepository;
        _walletContractRepository = walletContractRepository;
        _walletRepository = walletRepository;
        _installmentRepository = installmentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _outboxService = outboxService;
        _billingRepository = billingRepository;
        _transactionService = transactionService;
        _planRepository = planRepository;
        _distributedLockFactory = distributedLockFactory;
    }

    public async Task<OperationResult> CreateCustomerWalletByGranting(CgmProcessInstanceAddedEvent context)
    {
        var operationalFeeType = (OperationalFeeType)context.OperationalFeeType;
        var processInstanceDto = new LoanWalletProcessInstanceDto
        {
            TenantId = context.TenantId,
            CustomerId = context.CustomerId,
            CreditGrantingProcessId = context.CreditGrantingProcessId,
            NumberOfInstallment = context.NumberOfInstallment,
            InitialCreditAmount = context.InitialCreditAmount,
            CreditAmount = operationalFeeType == OperationalFeeType.AddToLoanInstallment ?
                context.InitialCreditAmount + context.OperationalFeeAmount : context.InitialCreditAmount,
            UserCreditGrantingProcessId = context.UserCreditGrantingProcessId,
            OperationalFeeType = operationalFeeType,
            OperationalFeeAmount = context.OperationalFeeAmount,
            VerificationFeeAmount = context.VerificationFeeAmount,
            PlanId = context.PlanId,
            SettlementType = WalletSettlementType.Cash
        };

        var tenant = await _tenantRepository.GetAsync(context.TenantId);
        WalletContract walletContract = await _walletContractRepository.GetByGrantingProcessIdAsync(context.CreditGrantingProcessId);
        Account tenantLoanAccount = await _accountRepository.GetAsync(AccountType.Loan, context.TenantId);
        List<Wallet> wallets = await _walletRepository.GetAsync(context.CustomerId, context.TenantId);

        var operationResult = await ValidateData(walletContract, tenantLoanAccount, wallets, processInstanceDto);
        if (!operationResult.IsSuccess)
        {
            return operationResult;
        }

        using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled); //todo

        await UpdatePlanAssignedCreditAmount(processInstanceDto);

        try
        {
            WalletContractPlan walletContractPlan = walletContract.WalletContractPlans.FirstOrDefault(x => x.PlanId == context.PlanId);
            processInstanceDto.Plan = walletContractPlan.Plan;
            processInstanceDto.TenantLoanAccount = tenantLoanAccount;
            processInstanceDto.WalletContractId = walletContract.Id;
            processInstanceDto.CurrencyTypeId = walletContractPlan.Plan.WalletConfiguration.CurrencyTypeId ?? 1;
            processInstanceDto.IsDefault = !wallets.Any(x => x.IsDefault && x.Status == WalletStatus.Active);

            await CreateWalletContractBusinessIdentity(processInstanceDto.CustomerId, walletContract);

            var walletId = await CreateCustomerWallet(tenant, processInstanceDto, transactionScope);

            _outboxService.AddNewEvent(new FcmProcessInstanceWalletIssuanceStatusUpdatedEvent
            {
                UserCreditGrantingProcessId = context.UserCreditGrantingProcessId,
                IsSuccess = true,
                Description = "کیف پول با موفقیت ایجاد شد.",
                WalletId = walletId
            });

            await _unitOfWork.SaveChangesAsync();

            transactionScope.Complete();

            return new OperationResult(true);
        }
        catch (Exception)
        {
            //_unitOfWork.ClearChangeTracker(); todo
            transactionScope.Dispose();
            throw;
        }
    }

    public async Task<OperationResult> CreateCustomerChequeWalletByGranting(CgmSettlementChequesProcessInstanceAddedEvent context)
    {
        var operationalFeeType = (OperationalFeeType)context.OperationalFeeType;

        var processInstanceDto = new LoanWalletProcessInstanceDto
        {
            TenantId = context.TenantId,
            CustomerId = context.CustomerId,
            CreditGrantingProcessId = context.CreditGrantingProcessId,
            NumberOfInstallment = context.NumberOfInstallment,
            InitialCreditAmount = context.InitialCreditAmount,
            CreditAmount = operationalFeeType == OperationalFeeType.AddToLoanInstallment ?
                context.InitialCreditAmount + context.OperationalFeeAmount : context.InitialCreditAmount,
            UserCreditGrantingProcessId = context.UserCreditGrantingProcessId,
            OperationalFeeType = operationalFeeType,
            OperationalFeeAmount = context.OperationalFeeAmount,
            VerificationFeeAmount = context.VerificationFeeAmount,
            PlanId = context.PlanId,
            SettlementType = WalletSettlementType.Cheque,
            SettlementChequeRegistrationDate = context.RegistrationDate,
            ChequeDetails = context.ChequeDetails
        };

        var tenant = await _tenantRepository.GetAsync(context.TenantId);
        WalletContract walletContract = await _walletContractRepository.GetByGrantingProcessIdAsync(context.CreditGrantingProcessId);
        Account tenantLoanAccount = await _accountRepository.GetAsync(AccountType.Loan, context.TenantId);
        List<Wallet> wallets = await _walletRepository.GetAsync(context.CustomerId, context.TenantId);

        var operationResult = await ValidateData(walletContract, tenantLoanAccount, wallets, processInstanceDto);
        if (!operationResult.IsSuccess)
        {
            return operationResult;
        }

        using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled); //todo

        await UpdatePlanAssignedCreditAmount(processInstanceDto);

        try
        {
            WalletContractPlan walletContractPlan = walletContract.WalletContractPlans.FirstOrDefault(x => x.PlanId == context.PlanId);

            processInstanceDto.Plan = walletContractPlan.Plan;
            processInstanceDto.TenantLoanAccount = tenantLoanAccount;
            processInstanceDto.WalletContractId = walletContract.Id;
            processInstanceDto.CurrencyTypeId = walletContractPlan.Plan.WalletConfiguration.CurrencyTypeId ?? 1;
            processInstanceDto.IsDefault = !wallets.Any(x => x.IsDefault && x.Status == WalletStatus.Active);

            await CreateWalletContractBusinessIdentity(processInstanceDto.CustomerId, walletContract);

            var walletId = await CreateCustomerWallet(tenant, processInstanceDto, transactionScope);

            _outboxService.AddNewEvent(new FcmProcessInstanceWalletIssuanceStatusUpdatedEvent
            {
                UserCreditGrantingProcessId = context.UserCreditGrantingProcessId,
                IsSuccess = true,
                Description = "کیف پول با موفقیت ایجاد شد.",
                WalletId = walletId
            });

            await _unitOfWork.SaveChangesAsync();

            transactionScope.Complete();

            return new OperationResult(true);
        }
        catch (Exception)
        {
            transactionScope.Dispose();
            throw;
        }
    }

    #region private methods

    private async Task<OperationResult> ValidateData(WalletContract walletContract, Account tenantLoanAccount, List<Wallet> wallets, LoanWalletProcessInstanceDto processInstanceDto)
    {
        if (processInstanceDto.InitialCreditAmount <= 0)
        {
            return new OperationResult(false, "مبلغ اعتبار نمیتواند کوچکتر مساوی صفر باشد.");
        }

        if (processInstanceDto.OperationalFeeAmount < 0)
        {
            return new OperationResult(false, "هزینه عملیات نمیتواند کوچکتر از صفر باشد.");
        }

        if (processInstanceDto.VerificationFeeAmount < 0)
        {
            return new OperationResult(false, "هزینه اعتبار سنجی نمیتواند کوچکتر از صفر باشد.");
        }

        if (walletContract == null)
        {
            return new OperationResult(false, "قراردادی برای این فرایند یافت نشد.");
        }

        //if (walletContract.Status != WalletContractStatus.Active)
        //{
        //    return new OperationResult(false, "قرارداد مربوط به این فرایند فعال نمی باشد.");
        //}

        if (walletContract.TenantId != processInstanceDto.TenantId)
        {
            return new OperationResult(false, "شناسه مالک زیرساخت تطبیق ندارد .");
        }

        WalletContractPlan walletContractPlan = walletContract.WalletContractPlans.FirstOrDefault(x => x.PlanId == processInstanceDto.PlanId);
        if (walletContractPlan == null)
        {
            return new OperationResult(false, "پلن یافت نشد.");
        }

        if (IsInvalidPlan(walletContractPlan, walletContract.TenantId))
        {
            return new OperationResult(false, "پلن به درستی تعریف نشده است.");
        }

        var customer = await _customerRepository.GetAsync(processInstanceDto.CustomerId);
        if (customer == null)
        {
            return new OperationResult(false, "مشتری یافت نشد.");
        }

        if (IsInvalidCustomer(walletContract, customer))
        {
            return new OperationResult(false, "مشتری به درستی تعریف نشده است.");
        }

        if (tenantLoanAccount == null)
        {
            return new OperationResult(false, "حساب اعتباری مالک زیرساخت یافت نشد.");
        }

        if (tenantLoanAccount.Status != AccountStatus.Active)
        {
            return new OperationResult(false, "حساب اعتباری مالک زیرساخت فعال نمی باشد.");
        }

        var planIds = walletContract.WalletContractPlans.Select(x => x.PlanId).ToList();

        if (wallets.Any(x => planIds.Contains(x.PlanId)))
        {
            return new OperationResult(false, "برای این مشتری قبلا کیف پول اعتباری ساخته شده است.");
        }

        if (processInstanceDto.SettlementType == WalletSettlementType.Cheque)
        {
            if (processInstanceDto.ChequeDetails.Count == 0)
            {
                return new OperationResult(false, "چک های تسویه الزامی می باشد.");
            }

            if (processInstanceDto.ChequeDetails.Count != processInstanceDto.NumberOfInstallment)
            {
                return new OperationResult(false, "تعداد چک های تسویه باید با تعداد اقساط برابر باشند.");
            }

            if (processInstanceDto.ChequeDetails.Sum(x => x.Amount) < processInstanceDto.InitialCreditAmount)
            {
                return new OperationResult(false, "مجموع مبلغ چک های تسویه نمی تواند کوچک تر از مبلغ اعتبار باشد.");
            }

            if (processInstanceDto.ChequeDetails.Any(x => x.Amount == 0))
            {
                return new OperationResult(false, "مبلغ چک تسویه نمی تواند صفر باشد");
            }
        }

        return new OperationResult(true);
    }

    private async Task<int> CreateCustomerWallet(Tenant tenant, LoanWalletProcessInstanceDto processInstanceDto, TransactionScope transactionScope)
    {
        var loanWalletAccount = new Account(processInstanceDto.CustomerId, tenant.Id, AccountType.LoanWallet, 0);
        await _accountRepository.AddAsync(loanWalletAccount);

        var loanWallet = new LoanWallet(processInstanceDto.CustomerId, processInstanceDto.TenantId, loanWalletAccount, processInstanceDto.Plan.Id,
            processInstanceDto.OperationalFeeAmount, processInstanceDto.OperationalFeeType, processInstanceDto.CreditAmount,
            processInstanceDto.NumberOfInstallment, processInstanceDto.WalletContractId, processInstanceDto.UserCreditGrantingProcessId, processInstanceDto.SettlementType);

        loanWallet.SetDefault(processInstanceDto.IsDefault);
        await _walletRepository.AddAsync(loanWallet);

        await _unitOfWork.SaveChangesAsync();

        var transaction = await CreateTransactions(loanWalletAccount, processInstanceDto, transactionScope);

        //todo raise event and Create Installments, long process && lock performance issues
        //maybe merge billing & installment creation
        if (processInstanceDto.SettlementType == WalletSettlementType.Cash)
        {
            await CreateInstallments(loanWalletAccount, transaction, processInstanceDto);
            await _unitOfWork.SaveChangesAsync();
        }
        else if (processInstanceDto.SettlementType == WalletSettlementType.Cheque)
        {
            var installments = await CreateChequeInstallments(loanWalletAccount, transaction, processInstanceDto);
            await _unitOfWork.SaveChangesAsync();

            PublishFcmSettlementChequesLoanWalletAddedEvent(processInstanceDto, loanWallet.Id, installments);
        }

        return loanWallet.Id;
    }

    private void PublishFcmSettlementChequesLoanWalletAddedEvent(LoanWalletProcessInstanceDto processInstanceDto, int loanWalletId, List<Installment> installments)
    {
        _outboxService.AddNewEvent(new FcmSettlementChequesLoanWalletAddedEvent
        {
            TenantId = processInstanceDto.TenantId,
            WalletId = loanWalletId,
            CurrencyTypeId = processInstanceDto.CurrencyTypeId,
            BusinessIdentityId = processInstanceDto.CustomerId,
            RegistrationDate = processInstanceDto.SettlementChequeRegistrationDate.Value,
            ChequeDetails = processInstanceDto.ChequeDetails.Select(x => new FcmChequeDetailsDto
            {
                SayyadIdentifier = x.SayyadIdentifier,
                Amount = x.Amount,
                DueDate = x.DueDate,
                ReceiverName = x.ReceiverName,
                ReceiverNationalId = x.ReceiverNationalId,
                Description = "چک تسویه بابت پرداخت اقساط " + processInstanceDto.Plan.Title + " کیف پول اعتباری ",
                InstallmentId = installments.First(z => DateOnly.FromDateTime(z.DueDate) == x.DueDate && z.Type == InstallmentType.Installment).Id
            }).ToList()
        });
    }

    private async Task<TransactionAggregate> CreateTransactions(Account customerWalletAccount, LoanWalletProcessInstanceDto processInstanceDto, TransactionScope transactionScope)
    {
        var newTransactions = new List<TransactionDto>()
        {
            new()
            {
                FromAccountId = processInstanceDto.TenantLoanAccount.Id,
                ToAccountId = customerWalletAccount.Id,
                TenantId = processInstanceDto.TenantId,
                Amount = processInstanceDto.CreditAmount,
                TransactionType = TransactionType.Charge,
                Description = TransactionDescriptionConstants.CHARGE_LOAN,
                Children = processInstanceDto.OperationalFeeAmount > 0 &&
                    (processInstanceDto.OperationalFeeType == OperationalFeeType.DeductFromLoan ||
                    processInstanceDto.OperationalFeeType == OperationalFeeType.AddToLoanInstallment) ?
                    [
                        new()
                        {
                            FromAccountId = customerWalletAccount.Id,
                            ToAccountId = processInstanceDto.TenantLoanAccount.Id,
                            TenantId = processInstanceDto.TenantId,
                            TransactionType = TransactionType.OperationalFee,
                            Description = TransactionDescriptionConstants.OPERATIONAL_FEE,
                            Amount = processInstanceDto.OperationalFeeAmount
                        }
                    ] : new()
            }
        };

        var transactions = await _transactionService.CreateTransactionAsync(newTransactions, transactionScope);
        var loanTransaction = transactions.FirstOrDefault();
        return loanTransaction;
    }

    private async Task CreateInstallments(Account customerWalletAccount, Domain.Core.Entities.TransactionAggregate.Transaction transaction, LoanWalletProcessInstanceDto processInstanceDto)
    {
        List<Installment> installments = [];
        var planDetailInstallment = processInstanceDto.Plan.PlanDetails.FirstOrDefault((Func<PlanDetail, bool>)(p => p.PlanDetailInstallments
            .Any((Func<PlanDetailInstallment, bool>)(d => d.NumberOfInstallment == processInstanceDto.NumberOfInstallment))));
        int gracePeriod = processInstanceDto.Plan.GracePeriod == null ? 0 : processInstanceDto.Plan.GracePeriod.Value;

        decimal interestPercent = 0;
        if (planDetailInstallment != null)
        {
            interestPercent = planDetailInstallment.InterestPercent ?? 0;
        }

        List<DateTime> installmentDates = installmentDates = DateHelper.CalculateInstallments(transaction.CreatedDateTime, processInstanceDto.Plan.BillingPeriod.GetValueOrDefault(), processInstanceDto.NumberOfInstallment,
                   processInstanceDto.Plan.InstallmentBreakType,
                   processInstanceDto.Plan.InstallmentBreak);

        var creditDetail = CreditDetailsCalculator.Calculate(processInstanceDto.CreditAmount, processInstanceDto.NumberOfInstallment, interestPercent);

        for (var i = 0; i < processInstanceDto.NumberOfInstallment; i++)
        {
            var currentInstallmentAmount = i < processInstanceDto.NumberOfInstallment - 1 ? creditDetail.InstallmentAmount : creditDetail.LastInstallmentAmount;

            var installment = new Installment(customerWalletAccount, processInstanceDto.TenantLoanAccount.Id,
                processInstanceDto.TenantId, currentInstallmentAmount, installmentDates[i], InstallmentType.Installment, InstallmentCategory.CustomerToTenant,
                DateHelper.GetInstallmentStartDate(installmentDates[i], processInstanceDto.Plan.BillingPeriod.GetValueOrDefault()), transaction, gracePeriod, processInstanceDto.SettlementType);

            installment.UpdateHasBilling(false);
            installment.SetNumber(i + 1);
            installments.Add(installment);

            if (creditDetail.InterestAmount > 0)
            {
                var currentInterestAmount = i < processInstanceDto.NumberOfInstallment - 1 ? creditDetail.InterestAmount : creditDetail.LastInterestAmount;

                var interest = new Installment(customerWalletAccount, processInstanceDto.TenantLoanAccount.Id,
                    processInstanceDto.TenantId, currentInterestAmount, installmentDates[i], InstallmentType.Interest, InstallmentCategory.CustomerToTenant,
                    DateHelper.GetInstallmentStartDate(installmentDates[i], processInstanceDto.Plan.BillingPeriod.GetValueOrDefault()), transaction, gracePeriod, processInstanceDto.SettlementType);
                interest.SetParent(installment);
                interest.UpdateHasBilling(false);
                installments.Add(interest);
            }
        }
        await _installmentRepository.AddRangeAsync(installments);
    }

    private async Task<List<Installment>> CreateChequeInstallments(Account customerWalletAccount, Domain.Core.Entities.TransactionAggregate.Transaction transaction, LoanWalletProcessInstanceDto processInstanceDto)
    {
        List<Installment> installments = [];
        var planDetailInstallment = processInstanceDto.Plan.PlanDetails.FirstOrDefault((Func<PlanDetail, bool>)(p => p.PlanDetailInstallments
            .Any((Func<PlanDetailInstallment, bool>)(d => d.NumberOfInstallment == processInstanceDto.NumberOfInstallment))));
        int gracePeriod = processInstanceDto.Plan.GracePeriod == null ? 0 : processInstanceDto.Plan.GracePeriod.Value;

        decimal interestPercent = 0;
        if (planDetailInstallment != null)
        {
            interestPercent = planDetailInstallment.InterestPercent ?? 0;
        }

        var checkDetails = processInstanceDto.ChequeDetails.OrderBy(x => x.DueDate).ToList();
        var creditDetail = CreditDetailsCalculator.Calculate(processInstanceDto.CreditAmount, processInstanceDto.NumberOfInstallment, interestPercent);

        if (checkDetails.Sum(x => x.Amount) != creditDetail.RepayableCreditAmount)
        {
            throw new ArgumentValidationException(nameof(creditDetail.RepayableCreditAmount), "مجموع مبالغ بازپرداختی با مجموع مبالغ اقساط برابر نمی باشد");
        }

        for (var i = 0; i < processInstanceDto.NumberOfInstallment; i++)
        {
            var checkDetail = checkDetails[i];
            decimal currentInterestAmount = 0;
            var currentInstallmentAmount = i < processInstanceDto.NumberOfInstallment - 1 ? creditDetail.InstallmentAmount : creditDetail.LastInstallmentAmount;

            var installment = new Installment(customerWalletAccount, processInstanceDto.TenantLoanAccount.Id,
                processInstanceDto.TenantId, currentInstallmentAmount, checkDetail.DueDate.ToDateTime(TimeOnly.MinValue), InstallmentType.Installment, InstallmentCategory.CustomerToTenant,
                DateHelper.GetInstallmentStartDate(checkDetail.DueDate.ToDateTime(TimeOnly.MinValue), processInstanceDto.Plan.BillingPeriod.GetValueOrDefault()), transaction, gracePeriod, processInstanceDto.SettlementType, checkDetail.SayyadIdentifier);

            installment.UpdateHasBilling(false);
            installment.SetNumber(i + 1);
            installments.Add(installment);

            if (creditDetail.InterestAmount > 0)
            {
                currentInterestAmount = i < processInstanceDto.NumberOfInstallment - 1 ? creditDetail.InterestAmount : creditDetail.LastInterestAmount;

                var interest = new Installment(customerWalletAccount, processInstanceDto.TenantLoanAccount.Id,
                    processInstanceDto.TenantId, currentInterestAmount, checkDetail.DueDate.ToDateTime(TimeOnly.MinValue), InstallmentType.Interest, InstallmentCategory.CustomerToTenant,
                    DateHelper.GetInstallmentStartDate(checkDetail.DueDate.ToDateTime(TimeOnly.MinValue), processInstanceDto.Plan.BillingPeriod.GetValueOrDefault()), transaction, gracePeriod, processInstanceDto.SettlementType, checkDetail.SayyadIdentifier);
                interest.SetParent(installment);
                interest.UpdateHasBilling(false);
                installments.Add(interest);
            }

            if (checkDetail.Amount != currentInstallmentAmount + currentInterestAmount)
            {
                throw new ArgumentValidationException(nameof(checkDetail.Amount), "مبلغ هر قسط با مبلغ چک برابر نمی باشد");
            }
        }
        await _installmentRepository.AddRangeAsync(installments);
        return installments;
    }

    private static bool IsInvalidCustomer(WalletContract walletContract, Customer customer)
    {
        return customer.TenantId != walletContract.TenantId ||
            !customer.CustomerOrganizations.Any(x => x.OrganizationId == walletContract.OrganizationId);
    }

    private static bool IsInvalidPlan(WalletContractPlan walletContractPlan, int tenantId)
    {
        return walletContractPlan.Plan == null ||
            walletContractPlan.Plan.WalletConfiguration == null ||
            walletContractPlan.Plan.WalletConfiguration.TenantId != tenantId;
    }

    private async Task UpdatePlanAssignedCreditAmount(LoanWalletProcessInstanceDto context)
    {
        var lockName = $"financialcore:plan:{context.PlanId}";

        using (var redLock = await _distributedLockFactory.CreateLockAsync(lockName,
            TimeSpan.FromSeconds(3),
            TimeSpan.FromSeconds(3),
            TimeSpan.FromSeconds(3)))
        {
            if (!redLock.IsAcquired)
                throw new Exception($"قفل:'{lockName}' بدست نیامد");

            var plan = await _planRepository.GetByIdAsync(context.PlanId);
            plan.IncreaseAssignedCreditAmount(context.InitialCreditAmount);
            _planRepository.Update(plan);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    private async Task CreateWalletContractBusinessIdentity(int customerId, WalletContract walletContract)
    {
        var walletContractBusinessIdentity = await _walletContractRepository.GetWalletContractBusinessIdentityAsync(walletContract.Id, customerId);
        if (walletContractBusinessIdentity == null)
        {
            walletContractBusinessIdentity = new WalletContractBusinessIdentity(walletContract.Id, customerId, true);
            await _walletContractRepository.AddWalletContractBusinessIdentityAsync(walletContractBusinessIdentity);
        }
    }
    #endregion
}