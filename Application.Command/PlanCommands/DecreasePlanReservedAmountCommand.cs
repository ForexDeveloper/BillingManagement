using Domain.Core.Entities.PlanAggregate;
using Domain.Core.Entities.PlanAggregate.Exceptions;
using Domain.Core.UnitOfWorkContracts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Polly;
using RedLockNet;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Command.PlanCommands;

public class DecreasePlanReservedAmountCommand : IRequest
{
    public DecreasePlanReservedAmountCommand(int planId, decimal amount)
    {
        PlanId = planId;
        Amount = amount;
    }

    public int PlanId { get; set; }
    public decimal Amount { get; set; }
}

public class DecreasePlanReservedAmountHandler : IRequestHandler<DecreasePlanReservedAmountCommand>
{
    private readonly IPlanRepository _planRepository;
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;
    private readonly IDistributedLockFactory _distributedLockFactory;

    public DecreasePlanReservedAmountHandler(IPlanRepository planRepository,
        IApplicationDbContextUnitOfWork unitOfWork,
        IDistributedLockFactory distributedLockFactory)
    {
        _planRepository = planRepository;
        _unitOfWork = unitOfWork;
        _distributedLockFactory = distributedLockFactory;

    }

    public async Task Handle(DecreasePlanReservedAmountCommand request, CancellationToken cancellationToken)
    {
        var lockName = $"financialcore:plan:{request.PlanId}";

        using var redLock = await _distributedLockFactory.CreateLockAsync(lockName,
            TimeSpan.FromSeconds(3),
            TimeSpan.FromSeconds(3),
            TimeSpan.FromSeconds(3));


        if (!redLock.IsAcquired)
            throw new Exception($"Lock:'{lockName}' could not be acquired");

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
            var plan = await _planRepository.GetByIdAsync(request.PlanId);

            if (plan == null)
                throw new PlanNotFoundException("طرح پیدا نشد.");

            plan.DecreaseReservedCreditAmount(request.Amount);

            _planRepository.Update(plan);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        });
    }
}