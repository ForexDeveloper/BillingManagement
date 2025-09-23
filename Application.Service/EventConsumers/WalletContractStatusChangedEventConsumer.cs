using Application.Service.Contracts;
using Domain.Core.Entities.WalletContractAggregate;
using Domain.Core.Enums;
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

public class WalletContractStatusChangedEventConsumer : IConsumer<FcmWalletContractStatusChangedEvent>
{
    private readonly IWalletContractRepository _walletContractRepository;
    private readonly IWalletContractService _walletContractService;
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;
    public readonly ILogger<WalletContractStatusChangedEventConsumer> _logger;

    public WalletContractStatusChangedEventConsumer(ILogger<WalletContractStatusChangedEventConsumer> logger,
        IApplicationDbContextUnitOfWork unitOfWork,
        IWalletContractRepository walletContractRepository,
        IWalletContractService walletContractService)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _walletContractRepository = walletContractRepository;
        _walletContractService = walletContractService;
    }

    public async Task Consume(ConsumeContext<FcmWalletContractStatusChangedEvent> context)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        bool succeed = true;
        try
        {
            var walletContract = await _walletContractRepository.GetAsync(context.Message.Id);
            if (walletContract == null)
            {
                return;
            }

            await UpdateWalletContractStatus(context, walletContract);
        }
        catch (Exception ex)
        {
            succeed = false;
            _logger.LogCritical(new LogStruct
            {
                Message = ex.Message,
                ServiceName = "WalletContractStatusChangedEventConsumer_Consume",
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
                ServiceName = "WalletContractStatusChangedEventConsumer_Consume",
                InputParams = context.Message,
                Results = succeed,
                ResponseTimeStopWatcher = stopWatch,
                Tags = LogMessageTag.EventBus,
            });
        }
    }

    private async Task UpdateWalletContractStatus(ConsumeContext<FcmWalletContractStatusChangedEvent> context, WalletContract currentContract)
    {
        if (context.Message.Status == (byte)WalletContractStatus.Active)
        {
            var contracts = await _walletContractRepository.GetByRootParentIdAsync(currentContract.RootParentId ?? currentContract.Id, WalletContractStatus.Active);

            contracts.ForEach(contract => contract.SetStatus(
                contract.Id == currentContract.Id ? WalletContractStatus.Active : WalletContractStatus.DeActive));

            _walletContractRepository.UpdateRange(contracts);
        }
        else
        {
            currentContract.SetStatus((WalletContractStatus)context.Message.Status);
            _walletContractRepository.Update(currentContract);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
