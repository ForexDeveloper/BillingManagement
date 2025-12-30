using System;
using MassTransit;
using System.Diagnostics;
using Shared.EventBus.Events;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Domain.Core.UnitOfWorkContracts;
using Shared.Logging.Abstraction.Models;
using Shared.Logging.Abstraction.Extensions;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.FinancierAggregate;

namespace Application.Service.EventConsumers;

public sealed class FinancierAddedOrUpdatedEventConsumer(
    IFinancierRepository financierRepository,
    IApplicationDbContextUnitOfWork unitOfWork,
    ILogger<FinancierAddedOrUpdatedEventConsumer> logger) : IConsumer<CmFinancierAddedOrUpdatedEvent>
{
    public async Task Consume(ConsumeContext<CmFinancierAddedOrUpdatedEvent> context)
    {
        var succeed = true;
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        const string SERVICE_NAME = $"{nameof(FinancierAddedOrUpdatedEventConsumer)}_{nameof(Consume)}";
        try
        {
            var financier = await financierRepository.GetAsync(context.Message.Id);

            if (financier == null)
            {
                await CreateFinancier(context);
            }
            else
            {
                await UpdateFinancier(context, financier);
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

    private async Task CreateFinancier(ConsumeContext<CmFinancierAddedOrUpdatedEvent> context)
    {
        Financier financier = new(context.Message.Id, context.Message.Name, context.Message.TenantId, context.Message.Type, context.Message.IsTenant);
        await financierRepository.AddAsync(financier);
        await unitOfWork.SaveChangesAsync();
    }

    private async Task UpdateFinancier(ConsumeContext<CmFinancierAddedOrUpdatedEvent> context, Financier financier)
    {
        financier.Update(context.Message.Name, context.Message.TenantId, context.Message.Type);
        financierRepository.Update(financier);
        await unitOfWork.SaveChangesAsync();
    }
}