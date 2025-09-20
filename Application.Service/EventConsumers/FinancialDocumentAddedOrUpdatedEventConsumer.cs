using Domain.Core.Entities.FinancialDocumentAggregate;
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

public class FinancialDocumentAddedOrUpdatedEventConsumer : IConsumer<FcmFinancialDocumentAddedOrUpdatedEvent>
{
    private readonly IFinancialDocumentRepository _financialDocumentRepository;
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;
    public readonly ILogger<FinancialDocumentAddedOrUpdatedEventConsumer> _logger;

    public FinancialDocumentAddedOrUpdatedEventConsumer(ILogger<FinancialDocumentAddedOrUpdatedEventConsumer> logger,
        IFinancialDocumentRepository financialDocumentRepository,
        IApplicationDbContextUnitOfWork unitOfWork)
    {
        _logger = logger;
        _financialDocumentRepository = financialDocumentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<FcmFinancialDocumentAddedOrUpdatedEvent> context)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        bool succeed = true;
        try
        {
            var financialDocument = await _financialDocumentRepository.GetByIdAsync(context.Message.Id);
            if (financialDocument == null)
            {
                await CreateFinancialDocument(context);
                return;
            }

            await UpdateFinancialDocument(context, financialDocument);
        }
        catch (Exception ex)
        {
            succeed = false;
            _logger.LogCritical(new LogStruct
            {
                Message = ex.Message,
                ServiceName = "FinancialDocumentAddedOrUpdatedEventConsumer_Consume",
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
                ServiceName = "FinancialDocumentAddedOrUpdatedEventConsumer_Consume",
                InputParams = context.Message,
                Results = succeed,
                ResponseTimeStopWatcher = stopWatch,
                Tags = LogMessageTag.EventBus,
            });
        }
    }

    private async Task CreateFinancialDocument(ConsumeContext<FcmFinancialDocumentAddedOrUpdatedEvent> context)
    {
        var financialDocument = new FinancialDocument(
                context.Message.Id,
                context.Message.FromBusinessIdentityId,
                context.Message.ToBusinessIdentityId,
                context.Message.TenantId,
                context.Message.Amount,
                context.Message.Type,
                context.Message.State,
                context.Message.PaymentGatewayType,
                context.Message.Description,
                context.Message.MerchantBranchId,
                context.Message.TenantMerchantContractId,
                context.Message.TenantPlatformContractId,
                context.Message.RefundReason,
                context.Message.RefundDescription,
                context.Message.RefundType,
                context.Message.ParentId
            );

        await _financialDocumentRepository.AddAsync(financialDocument);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task UpdateFinancialDocument(ConsumeContext<FcmFinancialDocumentAddedOrUpdatedEvent> context, FinancialDocument financialDocument)
    {
        financialDocument.SetState(context.Message.State);

        _financialDocumentRepository.Update(financialDocument);
        await _unitOfWork.SaveChangesAsync();
    }

}
