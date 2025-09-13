using Application.Service.Contracts;
using Application.Service.Dtos.BillingPayments;
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

public class PmSettlementChequePaymentUpdatedEventConsumer : IConsumer<PmSettlementChequePaymentUpdatedEvent>
{
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;
    public readonly ILogger<PmSettlementChequePaymentUpdatedEvent> _logger;
    public readonly IBillingPaymentService _billingPaymentService;
    private readonly IOutboxService _outboxService;

    public PmSettlementChequePaymentUpdatedEventConsumer(
        ILogger<PmSettlementChequePaymentUpdatedEvent> logger,
        IApplicationDbContextUnitOfWork unitOfWork,
        IBillingPaymentService billingPaymentService,
        IOutboxService outboxService)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _billingPaymentService = billingPaymentService;
        _outboxService = outboxService;
    }

    public async Task Consume(ConsumeContext<PmSettlementChequePaymentUpdatedEvent> context)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        bool succeed = true;
        try
        {
            await _billingPaymentService.CreateSettlemetChequePayment(new SettlementChequePaymentDto
            {
                CustomerId = context.Message.CustomerId,
                TenantId = context.Message.TenantId,
                InstallmentId = context.Message.InstallmentId,
                PaymentId = context.Message.PaymentId
            });
        }
        catch (Exception ex)
        {
            succeed = false;
            _logger.LogCritical(new LogStruct
            {
                Message = ex.Message,
                ServiceName = "PmSettlementChequePaymentUpdatedEventConsumer_Consume",
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
                ServiceName = "PmSettlementChequePaymentUpdatedEventConsumer_Consume",
                InputParams = context.Message,
                Results = succeed,
                ResponseTimeStopWatcher = stopWatch,
                Tags = LogMessageTag.EventBus,
            });
        }
    }
}