using Domain.Core.Entities.GuarantorAggregate;
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

public class GuarantorAddedOrUpdatedEventConsumer : IConsumer<CmGuarantorAddedOrUpdatedEvent>
{
    private readonly IGuarantorRepository _guarantorRepository;
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;
    public readonly ILogger<GuarantorAddedOrUpdatedEventConsumer> _logger;

    public GuarantorAddedOrUpdatedEventConsumer(ILogger<GuarantorAddedOrUpdatedEventConsumer> logger, IGuarantorRepository guarantorRepository,
        IApplicationDbContextUnitOfWork unitOfWork)
    {
        _logger = logger;
        _guarantorRepository = guarantorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<CmGuarantorAddedOrUpdatedEvent> context)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        bool succeed = true;
        try
        {
            var guarantor = await _guarantorRepository.GetAsync(context.Message.Id);
            if (guarantor == null)
            {
                await CreateGuarantor(context);
                return;
            }

            await UpdateGuarantor(context, guarantor);
        }
        catch (Exception ex)
        {
            succeed = false;
            _logger.LogCritical(new LogStruct
            {
                Message = ex.Message,
                ServiceName = "GuarantorAddedOrUpdatedEventConsumer_Consume",
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
                ServiceName = "GuarantorAddedOrUpdatedEventConsumer_Consume",
                InputParams = context.Message,
                Results = succeed,
                ResponseTimeStopWatcher = stopWatch,
                Tags = LogMessageTag.EventBus,
            });
        }
    }

    private async Task CreateGuarantor(ConsumeContext<CmGuarantorAddedOrUpdatedEvent> context)
    {
        Guarantor guarantor = new(context.Message.Id, context.Message.Name, context.Message.TenantId, context.Message.Type, context.Message.IsTenant);
        await _guarantorRepository.AddAsync(guarantor);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task UpdateGuarantor(ConsumeContext<CmGuarantorAddedOrUpdatedEvent> context, Guarantor guarantor)
    {
        guarantor.Update(context.Message.Name, context.Message.TenantId, context.Message.Type);
        _guarantorRepository.Update(guarantor);
        await _unitOfWork.SaveChangesAsync();
    }

}
