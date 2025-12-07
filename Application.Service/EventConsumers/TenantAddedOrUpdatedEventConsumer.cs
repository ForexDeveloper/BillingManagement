using Domain.Core.Entities.TenantAggregate;
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
using System.Threading.Tasks;


namespace Application.Service.EventConsumers;

public class TenantAddedOrUpdatedEventConsumer : IConsumer<CmTenantAddedOrUpdatedEvent>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;
    public readonly ILogger<TenantAddedOrUpdatedEventConsumer> _logger;

    public TenantAddedOrUpdatedEventConsumer(ILogger<TenantAddedOrUpdatedEventConsumer> logger, ITenantRepository tenantRepository,
        IApplicationDbContextUnitOfWork unitOfWork)
    {
        _logger = logger;
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<CmTenantAddedOrUpdatedEvent> context)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        bool succeed = true;
        try
        {
            var tenant = await _tenantRepository.GetAsync(context.Message.Id);
            if (tenant == null)
            {
                await CreateTenant(context);
                return;
            }

            await UpdateTenant(context, tenant);
        }
        catch (Exception ex)
        {
            succeed = false;
            _logger.LogCritical(new LogStruct
            {
                Message = ex.Message,
                ServiceName = "TenantAddedOrUpdatedEventConsumer_Consume",
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
                ServiceName = "TenantAddedOrUpdatedEventConsumer_Consume",
                InputParams = context.Message,
                Results = succeed,
                ResponseTimeStopWatcher = stopWatch,
                Tags = LogMessageTag.EventBus,
            });
        }
    }

    private async Task CreateTenant(ConsumeContext<CmTenantAddedOrUpdatedEvent> context)
    {
        Tenant tenant = new(context.Message.Id, context.Message.Title, context.Message.CreditProjectName, context.Message.BrandName, context.Message.InternalProjectManagerName, context.Message.HasCoWallet, context.Message.HasAnonymous);

        await _tenantRepository.AddAsync(tenant);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task UpdateTenant(ConsumeContext<CmTenantAddedOrUpdatedEvent> context, Tenant tenant)
    {
        tenant.Update(context.Message.Title, context.Message.CreditProjectName, context.Message.BrandName, context.Message.InternalProjectManagerName,context.Message.HasCoWallet,context.Message.HasAnonymous);
        _tenantRepository.Update(tenant);
        await _unitOfWork.SaveChangesAsync();
    }
}
