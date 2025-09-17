using Domain.Core.Entities.FinancierAggregate;
using Domain.Core.UnitOfWorkContracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.EventBus.Events;
using Shared.Logging.Abstraction.Extensions;
using Shared.Logging.Abstraction.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


namespace Application.Service.EventConsumers;

public class FinancierAddedOrUpdatedEventConsumer : IConsumer<CmFinancierAddedOrUpdatedEvent>
{
    private readonly IFinancierRepository _financierRepository;
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;
    public readonly ILogger<FinancierAddedOrUpdatedEventConsumer> _logger;

    public FinancierAddedOrUpdatedEventConsumer(ILogger<FinancierAddedOrUpdatedEventConsumer> logger, IFinancierRepository financierRepository,
        IApplicationDbContextUnitOfWork unitOfWork)
    {
        _logger = logger;
        _financierRepository = financierRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<CmFinancierAddedOrUpdatedEvent> context)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        bool succeed = true;
        try
        {
            var financier = await _financierRepository.GetAsync(context.Message.Id);
            if (financier == null)
            {
                await CreateFinancier(context);
                return;
            }

            await UpdateFinancier(context, financier);
        }
        catch (Exception ex)
        {
            succeed = false;
            _logger.LogCritical(new LogStruct
            {
                Message = ex.Message,
                ServiceName = "FinancierAddedOrUpdatedEventConsumer_Consume",
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
                ServiceName = "FinancierAddedOrUpdatedEventConsumer_Consume",
                InputParams = context.Message,
                Results = succeed,
                ResponseTimeStopWatcher = stopWatch,
                Tags = LogMessageTag.EventBus,
            });
        }
    }

    private async Task CreateFinancier(ConsumeContext<CmFinancierAddedOrUpdatedEvent> context)
    {
        Financier financier = new(context.Message.Id, context.Message.Name, context.Message.TenantId, context.Message.Type, context.Message.IsTenant);

        await _financierRepository.AddAsync(financier);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task UpdateFinancier(ConsumeContext<CmFinancierAddedOrUpdatedEvent> context, Financier financier)
    {
        financier.Update(context.Message.Name, context.Message.TenantId, context.Message.Type);
     
        _financierRepository.Update(financier);
        await _unitOfWork.SaveChangesAsync();
    }

}
