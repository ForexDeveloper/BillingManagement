using Domain.Core.Entities.GuarantorAggregate;
using Domain.Core.Entities.OrganizationAggregate;
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

public class CgmProcessAddedOrUpdatedEventConsumer : IConsumer<CgmProcessAddedOrUpdatedEvent>
{
    public readonly ILogger<CgmProcessAddedOrUpdatedEventConsumer> _logger;
    private readonly IWalletContractRepository _walletContractRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IGuarantorRepository _guarantorRepository;
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;

    public CgmProcessAddedOrUpdatedEventConsumer(
        ILogger<CgmProcessAddedOrUpdatedEventConsumer> logger,
        IWalletContractRepository walletContractRepository,
        IOrganizationRepository organizationRepository,
        IGuarantorRepository guarantorRepository,
        IApplicationDbContextUnitOfWork unitOfWork)
    {
        _logger = logger;
        _walletContractRepository = walletContractRepository;
        _organizationRepository = organizationRepository;
        _guarantorRepository = guarantorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<CgmProcessAddedOrUpdatedEvent> context)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        bool succeed = true;
        try
        {
            var walletContract = await _walletContractRepository.GetByGrantingProcessIdAsync(context.Message.Id, context.Message.TenantId);
            if (walletContract == null)
            {
                await CreateWalletContract(context);
                return;
            }

            await UpdateWalletContract(context, walletContract);
        }
        catch (Exception ex)
        {
            succeed = false;
            _logger.LogCritical(new LogStruct
            {
                Message = ex.Message,
                ServiceName = "CgmProcessAddedOrUpdatedEventConsumer_Consume",
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
                ServiceName = "CgmProcessAddedOrUpdatedEventConsumer_Consume",
                InputParams = context.Message,
                Results = succeed,
                ResponseTimeStopWatcher = stopWatch,
                Tags = LogMessageTag.EventBus,
            });
        }
    }

    private async Task CreateWalletContract(ConsumeContext<CgmProcessAddedOrUpdatedEvent> context)
    {
        var organization = await _organizationRepository.GetByTenantIdAsync(context.Message.TenantId);
        var walletContract = new WalletContract(context.Message.TenantId, null, organization.Id, DateTime.Now, null);
        SetWalletContractStatus(walletContract, context);
        SetWalletContractPlans(walletContract, context.Message.PlanIds);

        var guarantor = await _guarantorRepository.GetByTenantIdAsync(context.Message.TenantId);
        SetWalletContractGuarantors(walletContract, guarantor);

        walletContract.SetGrantingProcessId(context.Message.Id);

        await _walletContractRepository.AddAsync(walletContract);
        await _unitOfWork.SaveChangesAsync();
    }

    private static void SetWalletContractStatus(WalletContract walletContract, ConsumeContext<CgmProcessAddedOrUpdatedEvent> context)
    {
        if (context.Message.State == 2)
        {
            walletContract.SetStatus(WalletContractStatus.Active);
        }
        else
        {
            walletContract.SetStatus(WalletContractStatus.DeActive);
        }
    }

    private void SetWalletContractPlans(WalletContract walletContract, List<int>? planIds)
    {
        if (planIds == null || planIds.Count == 0)
        {
            return;
        }

        List<WalletContractPlan> walletContractPlans = [];
        walletContractPlans.AddRange(planIds.Select(customerId => new WalletContractPlan(walletContract.Id, customerId)));
        walletContract.SetWalletContractPlans(walletContractPlans);
    }

    private void SetWalletContractGuarantors(WalletContract walletContract, Guarantor? guarantor)
    {
        if (guarantor == null)
        {
            return;
        }

        List<WalletContractGuarantor> guarantors = [];
        List<WalletPortionType> portionTypes = Enum.GetValues(typeof(WalletPortionType)).Cast<WalletPortionType>().ToList();
        guarantors.Add(new WalletContractGuarantor(walletContract.Id, guarantor.Id, portionTypes));
        walletContract.SetWalletContractGuarantors(guarantors);
    }

    private async Task UpdateWalletContract(ConsumeContext<CgmProcessAddedOrUpdatedEvent> context, WalletContract walletContract)
    {
        SetWalletContractStatus(walletContract, context);
        UpdateWalletContractPlans(walletContract, context.Message.PlanIds);

        walletContract.SetEditDateTime(DateTime.UtcNow);
        _walletContractRepository.Update(walletContract);
        await _unitOfWork.SaveChangesAsync();
    }

    private void UpdateWalletContractPlans(WalletContract walletContract, List<int> planIds)
    {
        if (planIds == null)
        {
            throw new ArgumentNullException(nameof(planIds));
        }

        var oldContractPlans = walletContract.WalletContractPlans;
        var newContractPlans = new List<WalletContractPlan>();

        foreach (var planId in planIds)
        {
            if (oldContractPlans.All(x => x.PlanId != planId))
            {
                newContractPlans.Add(new WalletContractPlan(walletContract.Id, planId));
            }
        }

        foreach (var oldContractPlan in oldContractPlans)
        {
            if (!planIds.Contains(oldContractPlan.PlanId))
            {
                oldContractPlan.SetDeleted();
                oldContractPlan.SetEditDateTime(DateTime.Now);
            }
        }

        walletContract.SetWalletContractPlans(newContractPlans);
    }
}
