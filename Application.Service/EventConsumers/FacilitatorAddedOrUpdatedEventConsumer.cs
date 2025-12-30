using System;
using MassTransit;
using System.Diagnostics;
using System.Threading.Tasks;
using Shared.EventBus.Events;
using Microsoft.Extensions.Logging;
using Domain.Core.UnitOfWorkContracts;
using Shared.Logging.Abstraction.Models;
using Shared.Logging.Abstraction.Extensions;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.FacilitatorAggregate;

namespace Application.Service.EventConsumers;

public sealed class FacilitatorAddedOrUpdatedEventConsumer(
    IApplicationDbContextUnitOfWork unitOfWork,
    IFacilitatorRepository facilitatorRepository,
    ILogger<FacilitatorAddedOrUpdatedEventConsumer> logger) : IConsumer<CmFacilitatorAddedOrUpdatedEvent>
{
    public async Task Consume(ConsumeContext<CmFacilitatorAddedOrUpdatedEvent> context)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        bool succeed = true;
        const string SERVICE_NAME = $"{nameof(FacilitatorAddedOrUpdatedEventConsumer)}_{nameof(Consume)}";
        try
        {
            var facilitator = await facilitatorRepository.GetAsync(context.Message.Id);

            if (facilitator == null)
            {
                await CreateFacilitator(context);
            }
            else
            {
                await UpdateFacilitator(context, facilitator);
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

    private async Task CreateFacilitator(ConsumeContext<CmFacilitatorAddedOrUpdatedEvent> context)
    {
        Facilitator facilitator = new(context.Message.Id, context.Message.Name, context.Message.TenantId, context.Message.Type, context.Message.IsTenant);
        await facilitatorRepository.AddAsync(facilitator);
        await unitOfWork.SaveChangesAsync();
    }

    private async Task UpdateFacilitator(ConsumeContext<CmFacilitatorAddedOrUpdatedEvent> context, Facilitator facilitator)
    {
        facilitator.Update(context.Message.Name, context.Message.TenantId, context.Message.Type);
        facilitatorRepository.Update(facilitator);
        await unitOfWork.SaveChangesAsync();
    }
}