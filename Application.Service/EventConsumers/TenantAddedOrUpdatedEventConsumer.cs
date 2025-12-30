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

public sealed class TenantAddedOrUpdatedEventConsumer(
    ITenantRepository tenantRepository,
    IApplicationDbContextUnitOfWork unitOfWork,
    ILogger<TenantAddedOrUpdatedEventConsumer> logger) : IConsumer<CmTenantAddedOrUpdatedEvent>
{
    public async Task Consume(ConsumeContext<CmTenantAddedOrUpdatedEvent> context)
    {
        var succeed = true;
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        const string SERVICE_NAME = $"{nameof(TenantAddedOrUpdatedEventConsumer)}_{nameof(Consume)}";
        try
        {
            var tenant = await tenantRepository.GetAsync(context.Message.Id);

            if (tenant == null)
            {
                await CreateTenant(context);
            }
            else
            {
                await UpdateTenant(context, tenant);
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

    private async Task CreateTenant(ConsumeContext<CmTenantAddedOrUpdatedEvent> context)
    {
        Tenant tenant = new(context.Message.Id, context.Message.Title, context.Message.CreditProjectName, context.Message.BrandName, context.Message.InternalProjectManagerName, context.Message.HasCoWallet, context.Message.HasAnonymous);

        await tenantRepository.AddAsync(tenant);
        await unitOfWork.SaveChangesAsync();
    }

    private async Task UpdateTenant(ConsumeContext<CmTenantAddedOrUpdatedEvent> context, Tenant tenant)
    {
        tenant.Update(context.Message.Title, context.Message.CreditProjectName, context.Message.BrandName, context.Message.InternalProjectManagerName,context.Message.HasCoWallet,context.Message.HasAnonymous);
        tenantRepository.Update(tenant);
        await unitOfWork.SaveChangesAsync();
    }
}