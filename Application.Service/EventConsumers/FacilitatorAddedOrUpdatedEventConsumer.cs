using Domain.Core.Entities.FacilitatorAggregate;
using Domain.Core.UnitOfWorkContracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.EventBus.Events;
using Shared.Logging.Abstraction.Extensions;
using Shared.Logging.Abstraction.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;


namespace Application.Service.EventConsumers;

public class FacilitatorAddedOrUpdatedEventConsumer : IConsumer<CmFacilitatorAddedOrUpdatedEvent>
{
    private readonly IFacilitatorRepository _facilitatorRepository;
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;
    public readonly ILogger<FacilitatorAddedOrUpdatedEventConsumer> _logger;

    public FacilitatorAddedOrUpdatedEventConsumer(ILogger<FacilitatorAddedOrUpdatedEventConsumer> logger, IFacilitatorRepository facilitatorRepository,
        IApplicationDbContextUnitOfWork unitOfWork)
    {
        _logger = logger;
        _facilitatorRepository = facilitatorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<CmFacilitatorAddedOrUpdatedEvent> context)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        bool succeed = true;
        try
        {
            var facilitator = await _facilitatorRepository.GetAsync(context.Message.Id);
            if (facilitator == null)
            {
                await CreateFacilitator(context);
                return;
            }

            await UpdateFacilitator(context, facilitator);
        }
        catch (Exception ex)
        {
            succeed = false;
            _logger.LogCritical(new LogStruct
            {
                Message = ex.Message,
                ServiceName = "FacilitatorAddedOrUpdatedEventConsumer_Consume",
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
                ServiceName = "FacilitatorAddedOrUpdatedEventConsumer_Consume",
                InputParams = context.Message,
                Results = succeed,
                ResponseTimeStopWatcher = stopWatch,
                Tags = LogMessageTag.EventBus,
            });
        }
    }

    private async Task CreateFacilitator(ConsumeContext<CmFacilitatorAddedOrUpdatedEvent> context)
    {
        Facilitator facilitator = new(context.Message.Id, context.Message.Name, context.Message.TenantId, context.Message.Type, context.Message.IsTenant);
        await _facilitatorRepository.AddAsync(facilitator);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task UpdateFacilitator(ConsumeContext<CmFacilitatorAddedOrUpdatedEvent> context, Facilitator facilitator)
    {
        facilitator.Update(context.Message.Name, context.Message.TenantId, context.Message.Type);
        _facilitatorRepository.Update(facilitator);
        await _unitOfWork.SaveChangesAsync();
    }

}
