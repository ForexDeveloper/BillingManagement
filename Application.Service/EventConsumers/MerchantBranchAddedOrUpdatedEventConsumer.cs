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
using Domain.Core.Entities.MerchantAggregate;

namespace Application.Service.EventConsumers;

public sealed class MerchantBranchAddedOrUpdatedEventConsumer(
    IMerchantRepository merchantRepository,
    IApplicationDbContextUnitOfWork unitOfWork,
    ILogger<MerchantBranchAddedOrUpdatedEventConsumer> logger) : IConsumer<CmMerchantBranchAddedOrUpdatedEvent>
{
    public async Task Consume(ConsumeContext<CmMerchantBranchAddedOrUpdatedEvent> context)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        var succeed = true;
        const string SERVICE_NAME = $"{nameof(MerchantBranchAddedOrUpdatedEventConsumer)}_{nameof(Consume)}";
        try
        {
            //var merchant = await _merchantRepository.GetByIdAsync(context.Message.MerchantId);

            var merchantBranch = await merchantRepository.GetBranchAsync(context.Message.BranchId);

            if (merchantBranch == null)
            {
                await CreateMerchantBranch(context);
            }
            else
            {
                await UpdateMerchantBranch(context, merchantBranch);
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

    private async Task CreateMerchantBranch(ConsumeContext<CmMerchantBranchAddedOrUpdatedEvent> context)
    {
        var merchantBranch = new MerchantBranch(context.Message.BranchId, context.Message.MerchantId,
            context.Message.Title, context.Message.TerminalId);

        await merchantRepository.AddBranchAsync(merchantBranch);
        await unitOfWork.SaveChangesAsync();
    }

    private async Task UpdateMerchantBranch(ConsumeContext<CmMerchantBranchAddedOrUpdatedEvent> context, MerchantBranch merchantBranch)
    {
        merchantBranch.Update(context.Message.Title);
        merchantRepository.UpdateBranch(merchantBranch);
        await unitOfWork.SaveChangesAsync();
    }
}