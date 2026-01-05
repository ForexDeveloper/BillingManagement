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

public sealed class MerchantAddedOrUpdatedEventConsumer(
    IMerchantRepository merchantRepository,
    IApplicationDbContextUnitOfWork unitOfWork,
    ILogger<MerchantAddedOrUpdatedEventConsumer> logger) : IConsumer<CmMerchantAddedOrUpdatedEvent>
{
    public async Task Consume(ConsumeContext<CmMerchantAddedOrUpdatedEvent> context)
    {
        var succeed = true;
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        const string SERVICE_NAME = $"{nameof(MerchantAddedOrUpdatedEventConsumer)}_{nameof(Consume)}";
        try
        {
            var merchant = await merchantRepository.GetAsync(context.Message.MerchantId, context.Message.TenantId);

            if (merchant == null)
            {
                await CreateMerchant(context);
            }
            else
            {
                await UpdateMerchant(context, merchant);
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

    private async Task CreateMerchant(ConsumeContext<CmMerchantAddedOrUpdatedEvent> context)
    {
        var merchant = new Merchant(context.Message.MerchantId, context.Message.TenantId, context.Message.Title,
            context.Message.IdentityType, context.Message.SaleType, context.Message.Status);

        merchant.SetMerchantBranches(new MerchantBranch(context.Message.BranchId, context.Message.MerchantId, context.Message.Title, context.Message.TerminalId, context.Message.IsMerchant));

        await merchantRepository.AddAsync(merchant);
        await unitOfWork.SaveChangesAsync();
    }

    private async Task UpdateMerchant(ConsumeContext<CmMerchantAddedOrUpdatedEvent> context, Merchant merchant)
    {
        merchant.SetMerchant(context.Message.Title, context.Message.IdentityType,
            context.Message.SaleType, context.Message.Status);

        merchantRepository.Update(merchant);
        await unitOfWork.SaveChangesAsync();
    }
}