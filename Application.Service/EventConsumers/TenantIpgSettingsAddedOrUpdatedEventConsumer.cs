using Domain.Core.Entities.TenantAggregate;
using Domain.Core.UnitOfWorkContracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.EventBus.Events;
using Shared.Logging.Abstraction.Extensions;
using Shared.Logging.Abstraction.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Application.Service.EventConsumers;

public class TenantIpgSettingsAddedOrUpdatedEventConsumer : IConsumer<PmTenantIpgSettingAddedOrUpdatedEvent>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;
    public readonly ILogger<TenantIpgSettingsAddedOrUpdatedEventConsumer> _logger;

    public TenantIpgSettingsAddedOrUpdatedEventConsumer(ILogger<TenantIpgSettingsAddedOrUpdatedEventConsumer> logger,
        IApplicationDbContextUnitOfWork unitOfWork, ITenantRepository tenantRepository)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _tenantRepository = tenantRepository;
    }

    public async Task Consume(ConsumeContext<PmTenantIpgSettingAddedOrUpdatedEvent> context)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        bool succeed = true;
        try
        {
            var tenantIpgSetting = await _tenantRepository.TenantIPgSettingGetAsync(context.Message.Id);
            if (tenantIpgSetting == null)
            {
                await CreateTenantIpgSetting(context);
                return;
            }

            await UpdateTenantIpgSetting(context, tenantIpgSetting);
        }
        catch (Exception ex)
        {
            succeed = false;
            _logger.LogCritical(new LogStruct
            {
                Message = ex.Message,
                ServiceName = "TenantIpgSettingAddedOrUpdatedEventConsumer_Consume",
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
                ServiceName = "TenantIpgSettingAddedOrUpdatedEventConsumer_Consume",
                InputParams = context.Message,
                Results = succeed,
                ResponseTimeStopWatcher = stopWatch,
                Tags = LogMessageTag.EventBus,
            });
        }
    }

    private async Task CreateTenantIpgSetting(ConsumeContext<PmTenantIpgSettingAddedOrUpdatedEvent> context)
    {
        TenantIpgSetting tenantIpgSetting = new(context.Message.Id, context.Message.Title, context.Message.TenantId,
        context.Message.IpgType, context.Message.IsActive);
        await _tenantRepository.TenantIPgSettingAddAsync(tenantIpgSetting);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task UpdateTenantIpgSetting(ConsumeContext<PmTenantIpgSettingAddedOrUpdatedEvent> context, TenantIpgSetting tenantIpgSetting)
    {
        tenantIpgSetting.Update(context.Message.Title, context.Message.TenantId, context.Message.IpgType, context.Message.IsActive);

        _tenantRepository.TenantIPgSettingUpdate(tenantIpgSetting);
        await _unitOfWork.SaveChangesAsync();
    }
}
