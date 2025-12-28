using System;
using RedLockNet;
using MassTransit;
using System.Diagnostics;
using System.Threading.Tasks;
using Shared.EventBus.Events;
using Microsoft.Extensions.Logging;
using Application.Service.Contracts;
using Shared.Logging.Abstraction.Models;
using Shared.Logging.Abstraction.Extensions;
using Domain.Core.Entities.Shared.Exceptions;

namespace Application.Service.EventConsumers;

public sealed class BillingManualPaymentUpdateStateEventConsumer(
    IBillingPaymentService billingPaymentService,
    IDistributedLockFactory distributedLockFactory,
    ILogger<BillingManualPaymentUpdateStateEventConsumer> logger) : IConsumer<PmBillingManualPaymentUpdateStateEvent>
{
    public async Task Consume(ConsumeContext<PmBillingManualPaymentUpdateStateEvent> context)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        var succeed = true;
        const string SERVICE_NAME = "BillingManualPaymentUpdateStateEventConsumer_Consume";

        var lockName = $"billing:merchant:payment:{context.Message.BillingId}";
        await using var redLock = await distributedLockFactory.CreateLockAsync(lockName,
            TimeSpan.FromSeconds(3),
            TimeSpan.FromSeconds(6),
            TimeSpan.FromSeconds(3));

        if (!redLock.IsAcquired)
            throw new Exception($"Lock:'{lockName}' could not be acquired");

        try
        {
            await billingPaymentService.SetMerchantBillingPayment(context.Message);
        }
        catch (Exception exception)
        {
            succeed = false;

            if (exception is IBusinessException)
            {
                logger.LogWarning(new LogStruct
                {
                    Results = string.Empty,
                    Exception = exception,
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
                    Results = string.Empty,
                    Exception = exception,
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
                Tags = LogMessageTag.EventBus,
                InputParams = context.Message,
                ResponseTimeStopWatcher = stopWatch
            });
        }
    }
}