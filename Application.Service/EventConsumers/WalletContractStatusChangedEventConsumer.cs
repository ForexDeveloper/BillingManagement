using System;
using MassTransit;
using Domain.Core.Enums;
using System.Diagnostics;
using Shared.EventBus.Events;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Domain.Core.UnitOfWorkContracts;
using Shared.Logging.Abstraction.Models;
using Shared.Logging.Abstraction.Extensions;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.WalletContractAggregate;

namespace Application.Service.EventConsumers;

public sealed class WalletContractStatusChangedEventConsumer(
    IApplicationDbContextUnitOfWork unitOfWork,
    IWalletContractRepository walletContractRepository,
    ILogger<WalletContractStatusChangedEventConsumer> logger) : IConsumer<FcmWalletContractStatusChangedEvent>
{
    public async Task Consume(ConsumeContext<FcmWalletContractStatusChangedEvent> context)
    {
        var succeed = true;
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        const string SERVICE_NAME = $"{nameof(WalletContractStatusChangedEventConsumer)}_{nameof(Consume)}";
        try
        {
            var walletContract = await walletContractRepository.GetAsync(context.Message.Id);

            if (walletContract != null)
            {
                await UpdateWalletContractStatus(context, walletContract);
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

    private async Task UpdateWalletContractStatus(ConsumeContext<FcmWalletContractStatusChangedEvent> context, WalletContract currentContract)
    {
        if (context.Message.Status == (byte)WalletContractStatus.Active)
        {
            var contracts = await walletContractRepository.GetByRootParentIdAsync(currentContract.RootParentId ?? currentContract.Id, WalletContractStatus.Active);

            contracts.ForEach(contract => contract.SetStatus(
                contract.Id == currentContract.Id ? WalletContractStatus.Active : WalletContractStatus.DeActive));

            currentContract.SetStatus(WalletContractStatus.Active);
            walletContractRepository.UpdateRange(contracts);
        }
        else
        {
            currentContract.SetStatus((WalletContractStatus)context.Message.Status);
            walletContractRepository.Update(currentContract);
        }

        await unitOfWork.SaveChangesAsync();
    }
}