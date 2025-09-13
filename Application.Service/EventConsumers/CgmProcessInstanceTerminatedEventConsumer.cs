using Domain.Core.Entities.PlanAggregate;
using Domain.Core.Entities.PlanAggregate.Exceptions;
using Domain.Core.UnitOfWorkContracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using RedLockNet;
using Shared.EventBus.Events;
using Shared.Logging.Abstraction.Extensions;
using Shared.Logging.Abstraction.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Application.Service.EventConsumers;

public class CgmProcessInstanceTerminatedEventConsumer : IConsumer<CgmProcessInstanceTerminatedEvent>
{
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;
    private readonly ILogger<CgmProcessInstanceAddedEventConsumer> _logger;
    private readonly IPlanRepository _planRepository;

    private readonly IDistributedLockFactory _distributedLockFactory;
    public CgmProcessInstanceTerminatedEventConsumer(IPlanRepository planRepository,
        IApplicationDbContextUnitOfWork unitOfWork,
        ILogger<CgmProcessInstanceAddedEventConsumer> logger,
        IDistributedLockFactory distributedLockFactory)
    {
        _planRepository = planRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _distributedLockFactory = distributedLockFactory;
    }

    public async Task Consume(ConsumeContext<CgmProcessInstanceTerminatedEvent> context)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        bool succeed = true;

        var lockName = $"financialcore:plan:{context.Message.PlanId}";
        using var redLock = await _distributedLockFactory.CreateLockAsync(lockName,
            TimeSpan.FromSeconds(3),
            TimeSpan.FromSeconds(3),
            TimeSpan.FromSeconds(3));

        if (!redLock.IsAcquired)
            throw new Exception($"Lock:'{lockName}' could not be acquired");

        try
        {
            await UpdatePlan(context);
        }
        catch (Exception ex)
        {
            succeed = false;
            _logger.LogCritical(new LogStruct
            {
                Message = ex.Message,
                ServiceName = $"{nameof(CgmProcessInstanceTerminatedEvent)}_Consumer_Consume",
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
                ServiceName = $"{nameof(CgmProcessInstanceTerminatedEvent)}_Consumer_Consume",
                InputParams = context.Message,
                Results = succeed,
                ResponseTimeStopWatcher = stopWatch,
                Tags = LogMessageTag.EventBus,
            });
        }
    }

    private async Task UpdatePlan(ConsumeContext<CgmProcessInstanceTerminatedEvent> context)
    {
        var retryPolicy = Policy
        .Handle<DbUpdateConcurrencyException>()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: _ => TimeSpan.FromMilliseconds(200),
            onRetry: (ex, time, retryCount, ctx) =>
            {
                Console.WriteLine($"Concurrency conflict. Retrying {retryCount} after {time.TotalMilliseconds}ms...");
            });

        await retryPolicy.ExecuteAsync(async () =>
        {
            var plan = await _planRepository.GetByIdAsync(context.Message.PlanId);

            if (plan == null)
            {
                throw new PlanNotFoundException("طرح کیف پول پیدا نشد.");
            }

            plan.DecreaseReservedCreditAmount(context.Message.RequestedCreditAmount);

            _planRepository.Update(plan);

            await _unitOfWork.SaveChangesAsync();
        });
    }
}