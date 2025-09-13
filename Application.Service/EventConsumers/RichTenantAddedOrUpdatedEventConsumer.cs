using Application.Service.GlobalConstants;
using Domain.Core.AggregateRoots.WalletConfigurationAggregate;
using Domain.Core.Entities.AccountAggregate;
using Domain.Core.Entities.FacilitatorAggregate;
using Domain.Core.Entities.FinancierAggregate;
using Domain.Core.Entities.GuarantorAggregate;
using Domain.Core.Entities.OrganizationAggregate;
using Domain.Core.Entities.PlanAggregate;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.WalletContractAggregate;
using Domain.Core.Enums;
using Domain.Core.UnitOfWorkContracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.EventBus.Events;
using Shared.Logging.Abstraction.Extensions;
using Shared.Logging.Abstraction.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


namespace Application.Service.EventConsumers;

public class RichTenantAddedOrUpdatedEventConsumer : IConsumer<CmRichTenantAddedOrUpdatedEvent>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;
    public readonly ILogger<RichTenantAddedOrUpdatedEventConsumer> _logger;
    public readonly IFinancierRepository _financierRepository;
    public readonly IFacilitatorRepository _facilitatorRepository;
    public readonly IGuarantorRepository _guarantorRepository;
    public readonly IWalletConfigurationRepository _walletConfigurationRepository;

    public RichTenantAddedOrUpdatedEventConsumer(ILogger<RichTenantAddedOrUpdatedEventConsumer> logger, ITenantRepository tenantRepository,
        IApplicationDbContextUnitOfWork unitOfWork, IAccountRepository accountRepository, IOrganizationRepository organizationRepository, IFinancierRepository financierRepository, IFacilitatorRepository facilitatorRepository, IGuarantorRepository guarantorRepository, IWalletConfigurationRepository walletConfigurationRepository)
    {
        _logger = logger;
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
        _accountRepository = accountRepository;
        _organizationRepository = organizationRepository;
        _financierRepository = financierRepository;
        _facilitatorRepository = facilitatorRepository;
        _guarantorRepository = guarantorRepository;
        _walletConfigurationRepository = walletConfigurationRepository;
    }

    public async Task Consume(ConsumeContext<CmRichTenantAddedOrUpdatedEvent> context)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        bool succeed = true;
        try
        {
            var tenant = await _tenantRepository.GetAsync(context.Message.Id);
            if (tenant == null)
            {
                await CreateRichTenant(context);
                return;
            }

            await UpdateRichTenant(context.Message, tenant);
        }
        catch (Exception ex)
        {
            succeed = false;
            _logger.LogCritical(new LogStruct
            {
                Message = ex.Message,
                ServiceName = "RichTenantAddedOrUpdatedEventConsumer_Consume",
                InputParams = context.Message,
                Results = "",
                Exception = ex,
                ResponseTimeStopWatcher = stopWatch,
                Tags = LogMessageTag.EventBus
            });
            throw;
        }
        finally
        {
            _logger.LogTrace(new LogStruct
            {
                Message = "",
                ServiceName = "RichTenantAddedOrUpdatedEventConsumer_Consume",
                InputParams = context.Message,
                Results = succeed,
                ResponseTimeStopWatcher = stopWatch,
                Tags = LogMessageTag.EventBus,
            });
        }
    }

    private async Task CreateRichTenant(ConsumeContext<CmRichTenantAddedOrUpdatedEvent> context)
    {
        await CreateRichTenant(context.Message);
        await _unitOfWork.SaveChangesAsync();
    }
    private async Task CreateRichTenant(CmRichTenantAddedOrUpdatedEvent message)
    {
        Tenant tenant = new(message.Id, message.Title, message.CreditProjectName, message.BrandName, message.InternalProjectManagerName);
        await _tenantRepository.AddAsync(tenant);

        var accounts = new List<Account>
        {
            new(message.Id, message.Id, AccountType.Bank, 0),
            new(message.Id, message.Id, AccountType.CashWallet, 0),
            new(message.Id, message.Id, AccountType.Purchase, 0),
            new(message.Id, message.Id, AccountType.Commission , 0),
            new(message.Id, message.Id, AccountType.Loan  , 0),
            new(message.Id, message.Id, AccountType.BNPL  , 0),
            new(message.Id, message.Id, AccountType.Penalty  , 0),
            new(message.Id, message.Id, AccountType.Interest  , 0),
        };

        var otherAccounts = await CreateAccounts(message.FacilitatorId, message.GuarantorId, message.FinancierId, message.Id);
        accounts.AddRange(otherAccounts);

        await _accountRepository.AddRangeAsync(accounts);

        var organization = new Organization(message.OrganizationId, message.Title, message.Id);
        await _organizationRepository.AddAsync(organization);

        var financier = new Financier(message.FinancierId, message.Title, message.Id, message.IdentityType, true);
        await _financierRepository.AddAsync(financier);

        var facilitator = new Facilitator(message.FacilitatorId, message.Title, message.Id, message.IdentityType, true);
        await _facilitatorRepository.AddAsync(facilitator);

        var guarantor = new Guarantor(message.GuarantorId, message.Title, message.Id, message.IdentityType, true);
        await _guarantorRepository.AddAsync(guarantor);

        var walletConfiguration = new WalletConfiguration(WalletConfigurationConstants.CashTitle, message.Id,
           WalletType.Cash, null, null, WalletConfigurationConstants.MaxWallet, WalletConfigurationConstants.MaxTotalCredit);

        var plan = new Plan(walletConfiguration.Id, PLanConstants.CashTitle, PLanConstants.MaxDailyWithdrawal,
            PLanConstants.MaxWallet, PLanConstants.MaxTotalCredit, PLanConstants.MaxDailyDeposit);

        var walletCantract = new WalletContract(message.Id, null, organization.Id, DateTime.Now, null, WalletContractStatus.Active);

        plan.SetWalletContractPlan(new WalletContractPlan(walletCantract, plan));

        walletConfiguration.SetPlan(plan);

        await _walletConfigurationRepository.AddAsync(walletConfiguration);

    }

    private async Task<List<Account>> CreateAccounts(int facilitatorId, int guarantorId, int financierId, int tenantId)
    {
        var existingAccounts = await _accountRepository.GetByBusinessIdentityIds([facilitatorId, guarantorId, financierId], tenantId);
        List<Account> accounts = [];

        if (!existingAccounts.Any(x => x.BusinessIdentityId == facilitatorId && x.Type == AccountType.Commission))
        {
            var account = new Account(facilitatorId, tenantId, AccountType.Commission, 0);
            accounts.Add(account);
        }

        if (!existingAccounts.Any(x => x.BusinessIdentityId == facilitatorId && x.Type == AccountType.Bank))
        {
            var account = new Account(facilitatorId, tenantId, AccountType.Bank, 0);
            accounts.Add(account);
        }

        if (!existingAccounts.Any(x => x.BusinessIdentityId == financierId && x.Type == AccountType.Commission))
        {
            var account = new Account(financierId, tenantId, AccountType.Commission, 0);
            accounts.Add(account);
        }

        if (!existingAccounts.Any(x => x.BusinessIdentityId == financierId && x.Type == AccountType.Bank))
        {
            var account = new Account(financierId, tenantId, AccountType.Bank, 0);
            accounts.Add(account);
        }

        if (!existingAccounts.Any(x => x.BusinessIdentityId == guarantorId && x.Type == AccountType.Commission))
        {
            var account = new Account(guarantorId, tenantId, AccountType.Commission, 0);
            accounts.Add(account);
        }

        if (!existingAccounts.Any(x => x.BusinessIdentityId == guarantorId && x.Type == AccountType.Bank))
        {
            var account = new Account(guarantorId, tenantId, AccountType.Bank, 0);
            accounts.Add(account);
        }

        return accounts;
    }

    private async Task UpdateRichTenant(CmRichTenantAddedOrUpdatedEvent message, Tenant tenant)
    {
        tenant.Update(message.Title, message.CreditProjectName, message.BrandName, message.InternalProjectManagerName);
        _tenantRepository.Update(tenant);

        var organization = await _organizationRepository.GetAsync(message.OrganizationId);
        var financier = await _financierRepository.GetAsync(message.FinancierId);
        var facilitator = await _facilitatorRepository.GetAsync(message.FacilitatorId);
        var guarantor = await _guarantorRepository.GetAsync(message.GuarantorId);

        if (organization is not null && organization.Title != message.Title)
        {
            organization.SetTitle(message.Title);
            _organizationRepository.Update(organization);
        }

        if (financier is not null && financier.Name != message.Title)
        {
            financier.SetName(message.Title);
            _financierRepository.Update(financier);
        }

        if (facilitator is not null && facilitator.Name != message.Title)
        {
            facilitator.SetName(message.Title);
            _facilitatorRepository.Update(facilitator);
        }

        if (guarantor is not null && guarantor.Name != message.Title)
        {
            guarantor.SetName(message.Title);
            _guarantorRepository.Update(guarantor);
        }

        var otherAccounts = await CreateAccounts(message.FacilitatorId, message.GuarantorId, message.FinancierId, message.Id);
        if (otherAccounts.Count > 0)
        {
            await _accountRepository.AddRangeAsync(otherAccounts);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
