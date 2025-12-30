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
using Domain.Core.Entities.GuarantorAggregate;

namespace Application.Service.EventConsumers;

public sealed class GuarantorAddedOrUpdatedEventConsumer(
    IGuarantorRepository guarantorRepository,
    IApplicationDbContextUnitOfWork unitOfWork,
    ILogger<GuarantorAddedOrUpdatedEventConsumer> logger) : IConsumer<CmGuarantorAddedOrUpdatedEvent>
{
    public async Task Consume(ConsumeContext<CmGuarantorAddedOrUpdatedEvent> context)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        var succeed = true;
        const string SERVICE_NAME = $"{nameof(GuarantorAddedOrUpdatedEventConsumer)}_{nameof(Consume)}";
        try
        {
            var guarantor = await guarantorRepository.GetAsync(context.Message.Id);

            if (guarantor == null)
            {
                await CreateGuarantor(context);
            }
            else
            {
                await UpdateGuarantor(context, guarantor);
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

    private async Task CreateGuarantor(ConsumeContext<CmGuarantorAddedOrUpdatedEvent> context)
    {
        Guarantor guarantor = new(context.Message.Id, context.Message.Name, context.Message.TenantId, context.Message.Type, context.Message.IsTenant);
        await guarantorRepository.AddAsync(guarantor);
        await unitOfWork.SaveChangesAsync();
    }

    private async Task UpdateGuarantor(ConsumeContext<CmGuarantorAddedOrUpdatedEvent> context, Guarantor guarantor)
    {
        guarantor.Update(context.Message.Name, context.Message.TenantId, context.Message.Type);
        guarantorRepository.Update(guarantor);
        await unitOfWork.SaveChangesAsync();
    }
}