using System;
using MassTransit;
using System.Linq;
using Domain.Core.Enums;
using System.Diagnostics;
using Shared.EventBus.Events;
using System.Threading.Tasks;
using System.Collections.Generic;
using Application.Service.Contracts;
using Application.Service.Helper;
using Microsoft.Extensions.Logging;
using Domain.Core.UnitOfWorkContracts;
using Shared.Logging.Abstraction.Models;
using Shared.Logging.Abstraction.Extensions;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.MerchantInstallmentAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate;

namespace Application.Service.EventConsumers;

public sealed class FinancialDocumentAddedOrUpdatedEventConsumer(
    IApplicationDbContextUnitOfWork unitOfWork,
    IMerchantInstallmentService merchantInstallmentService,
    IFinancialDocumentRepository financialDocumentRepository,
    ITenantMerchantContractRepository tenantMerchantContractRepository,
    ILogger<FinancialDocumentAddedOrUpdatedEventConsumer> logger) : IConsumer<FcmFinancialDocumentAddedOrUpdatedEvent>
{
    public async Task Consume(ConsumeContext<FcmFinancialDocumentAddedOrUpdatedEvent> context)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        bool succeed = true;
        try
        {
            var financialDocument = await financialDocumentRepository.GetByIdAsync(context.Message.Id);

            if (financialDocument == null)
            {
                //purchase and refund

                financialDocument = await CreateFinancialDocument(context);

                var contract = await tenantMerchantContractRepository.GetActiveContractAsync(
                    financialDocument.TenantId, financialDocument.ToBusinessIdentityId);

                if (financialDocument.Type == FinancialDocumentType.Purchase)
                {
                    var commission = await merchantInstallmentService.CreateInstallments(contract, financialDocument);

                    financialDocument.SetCommission(commission);
                }

                //only reverse
                await UpdateFinancialDocument(context, financialDocument);

                await unitOfWork.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            succeed = false;
            logger.LogCritical(new LogStruct
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
            logger.LogTrace(new LogStruct
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

    private async Task<FinancialDocument> CreateFinancialDocument(ConsumeContext<FcmFinancialDocumentAddedOrUpdatedEvent> context)
    {
        var type = FinancialDocumentType.Purchase;

        //6 => refund
        if (context.Message.Type == 6)
        {
            type = FinancialDocumentType.Refund;
        }

        var financialDocument = new FinancialDocument(
                context.Message.Id,
                context.Message.FromBusinessIdentityId,
                context.Message.ToBusinessIdentityId,
                context.Message.TenantId,
                context.Message.Amount,
                context.Message.CreditAmount,
                context.Message.CashAmount,
                context.Message.PrepaymentAmount,
                type,
                (FinancialDocumentState)context.Message.State,
                (PaymentGatewayType)context.Message.PaymentGatewayType,
                context.Message.Description,
                context.Message.MerchantBranchId,
                context.Message.TenantMerchantContractId,
                context.Message.TenantPlatformContractId,
                context.Message.RefundReason != null ? (RefundReason)context.Message.RefundReason : null,
                context.Message.RefundDescription,
                context.Message.RefundType != null ? (FinancialDocumentRefundType)context.Message.RefundType : null,
                context.Message.ParentId
            );

        await financialDocumentRepository.AddAsync(financialDocument);

        return financialDocument;
    }

    private async Task UpdateFinancialDocument(ConsumeContext<FcmFinancialDocumentAddedOrUpdatedEvent> context, FinancialDocument financialDocument)
    {
        FinancialDocumentType type = FinancialDocumentType.Reverse;

        //1 => purchase
        if (context.Message.Type != 1)
        {
            ArgumentValidationException ex = new ArgumentValidationException(nameof(context.Message.Type), "نوع سند مالی برای بازگشت کامل وجه خرید معتبر نیست.");
            logger.LogError(new LogStruct
            {
                Message = ex.Message,
                ServiceName = "FinancialDocumentAddedOrUpdatedEventConsumer_Consume",
                InputParams = context.Message,
                Results = "",
                Exception = ex,
                Tags = LogMessageTag.EventBus
            });

            throw ex;
        }

        financialDocument.SetState(FinancialDocumentState.Reverse);

        financialDocumentRepository.Update(financialDocument);
        await unitOfWork.SaveChangesAsync();
    }
}