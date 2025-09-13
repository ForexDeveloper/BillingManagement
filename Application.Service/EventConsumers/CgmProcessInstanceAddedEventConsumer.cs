using Application.Service.Contracts;
using Domain.Core.UnitOfWorkContracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.EventBus.Contracts;
using Shared.EventBus.Events;
using Shared.Logging.Abstraction.Extensions;
using Shared.Logging.Abstraction.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Application.Service.EventConsumers;
public class CgmProcessInstanceAddedEventConsumer : IConsumer<CgmProcessInstanceAddedEvent>
{
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;
    public readonly ILogger<CgmProcessInstanceAddedEventConsumer> _logger;
    public readonly ILoanWalletService _walletService;
    private readonly IOutboxService _outboxService;
    public CgmProcessInstanceAddedEventConsumer(ILogger<CgmProcessInstanceAddedEventConsumer> logger,
        IApplicationDbContextUnitOfWork unitOfWork, ILoanWalletService walletService, IOutboxService outboxService)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _walletService = walletService;
        _outboxService = outboxService;

    }

    public async Task Consume(ConsumeContext<CgmProcessInstanceAddedEvent> context)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        bool succeed = true;
        try
        {
            var result = await _walletService.CreateCustomerWalletByGranting(context.Message);

            if (!result.IsSuccess)
            {
                _logger.LogError(new LogStruct
                {
                    Message = result.ErrorMessage,
                    ServiceName = "ProcessInstanceAddedEventConsumer_Consume",
                    InputParams = context.Message,
                    Results = "",
                    Exception = null,
                    ResponseTimeStopWatcher = stopWatch,
                    Tags = LogMessageTag.EventBus
                });

                _outboxService.AddNewEvent(new FcmProcessInstanceWalletIssuanceStatusUpdatedEvent
                {
                    UserCreditGrantingProcessId = context.Message.UserCreditGrantingProcessId,
                    IsSuccess = false,
                    Description = result.ErrorMessage,
                    WalletId = null
                });

                await _unitOfWork.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            succeed = false;
            _logger.LogCritical(new LogStruct
            {
                Message = ex.Message,
                ServiceName = "ProcessInstanceAddedEventConsumer_Consume",
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
                ServiceName = "ProcessInstanceAddedEventConsumer_Consume",
                InputParams = context.Message,
                Results = succeed,
                ResponseTimeStopWatcher = stopWatch,
                Tags = LogMessageTag.EventBus,
            });
        }
    }
}