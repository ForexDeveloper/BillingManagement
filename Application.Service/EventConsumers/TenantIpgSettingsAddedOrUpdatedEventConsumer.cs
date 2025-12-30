using System;
using MassTransit;
using System.Diagnostics;
using System.Threading.Tasks;
using Shared.EventBus.Events;
using Microsoft.Extensions.Logging;
using Domain.Core.UnitOfWorkContracts;
using Shared.Logging.Abstraction.Models;
using Domain.Core.Entities.TenantAggregate;
using Shared.Logging.Abstraction.Extensions;
using Domain.Core.Entities.Shared.Exceptions;

namespace Application.Service.EventConsumers;

public sealed class TenantIpgSettingsAddedOrUpdatedEventConsumer(
    ITenantRepository tenantRepository,
    IApplicationDbContextUnitOfWork unitOfWork,
    ILogger<TenantIpgSettingsAddedOrUpdatedEventConsumer> logger) : IConsumer<PmTenantIpgSettingAddedOrUpdatedEvent>
{
    public async Task Consume(ConsumeContext<PmTenantIpgSettingAddedOrUpdatedEvent> context)
    {
        var succeed = true;
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        const string SERVICE_NAME = $"{nameof(TenantIpgSettingsAddedOrUpdatedEventConsumer)}_{nameof(Consume)}";
        try
        {
            var tenantIpgSetting = await tenantRepository.TenantIPgSettingGetAsync(context.Message.Id);

            if (tenantIpgSetting == null)
            {
                await CreateTenantIpgSetting(context);
            }
            else
            {
                await UpdateTenantIpgSetting(context, tenantIpgSetting);
            }
        }
        catch (Exception exception)
        {
            succeed = false;

            if (exception is IBusinessException)
            {
                logger.LogWarning(new LogStruct
                {
                    Exception = exception,
                    Results = string.Empty,
                    ServiceName = SERVICE_NAME,
                    Message = exception.Message,
                    InputParams = context.Message,
                    Tags = LogMessageTag.EventBus,
                    ResponseTimeStopWatcher = stopWatch
                });
            }
            else
            {
                logger.LogCritical(new LogStruct
                {
                    Exception = exception,
                    Results = string.Empty,
                    ServiceName = SERVICE_NAME,
                    Message = exception.Message,
                    InputParams = context.Message,
                    Tags = LogMessageTag.EventBus,
                    ResponseTimeStopWatcher = stopWatch
                });

                throw;
            }
        }
        finally
        {
            logger.LogTrace(new LogStruct
            {
                Results = succeed,
                Message = string.Empty,
                ServiceName = SERVICE_NAME,
                InputParams = context.Message,
                Tags = LogMessageTag.EventBus,
                ResponseTimeStopWatcher = stopWatch
            });
        }
    }

    private async Task CreateTenantIpgSetting(ConsumeContext<PmTenantIpgSettingAddedOrUpdatedEvent> context)
    {
        TenantIpgSetting tenantIpgSetting = new(context.Message.Id, context.Message.Title, context.Message.TenantId,
        context.Message.IpgType, context.Message.IsActive);
        await tenantRepository.TenantIPgSettingAddAsync(tenantIpgSetting);
        await unitOfWork.SaveChangesAsync();
    }

    private async Task UpdateTenantIpgSetting(ConsumeContext<PmTenantIpgSettingAddedOrUpdatedEvent> context, TenantIpgSetting tenantIpgSetting)
    {
        tenantIpgSetting.Update(context.Message.Title, context.Message.TenantId, context.Message.IpgType, context.Message.IsActive);
        tenantRepository.TenantIPgSettingUpdate(tenantIpgSetting);
        await unitOfWork.SaveChangesAsync();
    }
}