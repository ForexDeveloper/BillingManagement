using Application.Service.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using RedLockNet;
using Shared.EventBus.Events;
using Shared.Logging.Abstraction.Extensions;
using Shared.Logging.Abstraction.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Application.Service.EventConsumers;

public class BillingManualPaymentUpdateStateEventConsumer : IConsumer<PmBillingManualPaymentUpdateStateEvent>
{
    private readonly IBillingPaymentService _billingPaymentService;
    public readonly ILogger<BillingManualPaymentUpdateStateEventConsumer> _logger;
    private readonly IDistributedLockFactory _distributedLockFactory;

    public BillingManualPaymentUpdateStateEventConsumer(ILogger<BillingManualPaymentUpdateStateEventConsumer> logger,
        IBillingPaymentService billingPaymentService,
        IDistributedLockFactory distributedLockFactory)
    {
        _logger = logger;
        _billingPaymentService = billingPaymentService;
        _distributedLockFactory = distributedLockFactory;
    }

    public async Task Consume(ConsumeContext<PmBillingManualPaymentUpdateStateEvent> context)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        bool succeed = true;

        var lockName = $"billing:payment:{context.Message.BillId}";
        using var redLock = await _distributedLockFactory.CreateLockAsync(lockName,
            TimeSpan.FromSeconds(3),
            TimeSpan.FromSeconds(6),
            TimeSpan.FromSeconds(3));

        if (!redLock.IsAcquired)
            throw new Exception($"Lock:'{lockName}' could not be acquired");

        try
        {
            await _billingPaymentService.MerchantBillingPayment(context.Message);
        }
        catch (Exception ex)
        {
            succeed = false;
            _logger.LogCritical(new LogStruct
            {
                Message = ex.Message,
                ServiceName = "BillingManualPaymentUpdateStateEventConsumer_Consume",
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
                ServiceName = "BillingManualPaymentUpdateStateEventConsumer_Consume",
                InputParams = context.Message,
                Results = succeed,
                ResponseTimeStopWatcher = stopWatch,
                Tags = LogMessageTag.EventBus,
            });
        }
    }
}
