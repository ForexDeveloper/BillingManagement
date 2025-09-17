using Domain.Core.Entities.MerchantAggregate;
using Domain.Core.Enums;
using Domain.Core.UnitOfWorkContracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.EventBus.Events;
using Shared.Logging.Abstraction.Extensions;
using Shared.Logging.Abstraction.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Service.EventConsumers;

public class MerchantBranchAddedOrUpdatedEventConsumer : IConsumer<CmMerchantBranchAddedOrUpdatedEvent>
{
    private readonly IMerchantRepository _merchantRepository;
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;
    public readonly ILogger<MerchantBranchAddedOrUpdatedEventConsumer> _logger;

    public MerchantBranchAddedOrUpdatedEventConsumer(ILogger<MerchantBranchAddedOrUpdatedEventConsumer> logger, 
        IApplicationDbContextUnitOfWork unitOfWork, IMerchantRepository merchantRepository)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _merchantRepository = merchantRepository;
    }

    public async Task Consume(ConsumeContext<CmMerchantBranchAddedOrUpdatedEvent> context)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        bool succeed = true;
        try
        {
            //var merchant = await _merchantRepository.GetByIdAsync(context.Message.MerchantId);
            var merchantBranch = await _merchantRepository.GetBranchAsync(context.Message.BranchId);
            if (merchantBranch == null)
            {
                await CreateMerchantBranch(context);
                return;
            }

            await UpdateMerchantBranch(context, merchantBranch);
        }
        catch (Exception ex)
        {
            succeed = false;
            _logger.LogCritical(new LogStruct
            {
                Message = ex.Message,
                ServiceName = "MerchantBranchAddedOrUpdatedEventConsumer_Consume",
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
                ServiceName = "MerchantBranchAddedOrUpdatedEventConsumer_Consume",
                InputParams = context.Message,
                Results = succeed,
                ResponseTimeStopWatcher = stopWatch,
                Tags = LogMessageTag.EventBus,
            });
        }
    }

    private async Task CreateMerchantBranch(ConsumeContext<CmMerchantBranchAddedOrUpdatedEvent> context)
    {
        var merchantBranch = new MerchantBranch(context.Message.BranchId, context.Message.MerchantId,
            context.Message.Title, context.Message.TerminalId);

        await _merchantRepository.AddBranchAsync(merchantBranch);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task UpdateMerchantBranch(ConsumeContext<CmMerchantBranchAddedOrUpdatedEvent> context, MerchantBranch merchantBranch)
    {
        merchantBranch.Update(context.Message.Title);
        _merchantRepository.UpdateBranch(merchantBranch);
        await _unitOfWork.SaveChangesAsync();
    }
}
