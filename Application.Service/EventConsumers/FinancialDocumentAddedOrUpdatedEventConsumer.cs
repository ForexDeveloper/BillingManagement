using System;
using MassTransit;
using Domain.Core.Enums;
using System.Diagnostics;
using System.Threading.Tasks;
using Shared.EventBus.Events;
using Application.Service.Helper;
using Microsoft.Extensions.Logging;
using Application.Service.Contracts;
using Domain.Core.UnitOfWorkContracts;
using Shared.Logging.Abstraction.Models;
using Shared.Logging.Abstraction.Extensions;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.FinancialDocumentAggregate;

namespace Application.Service.EventConsumers;

public sealed class FinancialDocumentAddedOrUpdatedEventConsumer(
    IApplicationDbContextUnitOfWork unitOfWork,
    IMerchantInstallmentService merchantInstallmentService,
    IFinancialDocumentRepository financialDocumentRepository,
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
                decimal commission;

                //purchase and refund
                financialDocument = await CreateFinancialDocument(context);

                if (financialDocument.Type == FinancialDocumentType.Purchase)
                { 
                    commission = await merchantInstallmentService.CreateInstallments(financialDocument);
                }
                else
                {
                    var parentId = financialDocument.ParentId!.Value;

                    var purchaseCommission = await financialDocumentRepository.GetPurchaseCommission(parentId);

                    var purchaseTransaction = await financialDocumentRepository.GetPurchaseTransaction(parentId);

                    var sumOfRefundTransactions = await financialDocumentRepository.GetSumOfRefundTransactions(parentId);

                    if (purchaseTransaction ==  sumOfRefundTransactions + financialDocument.Amount)
                    {
                        var sumOfRefundCommissions = await financialDocumentRepository.GetSumOfRefundCommissions(parentId);

                        commission = purchaseCommission - sumOfRefundCommissions;
                    }
                    else
                    {
                        commission = RoundHelper.RoundAmount(purchaseCommission * financialDocument.Amount / purchaseTransaction);
                    }
                }

                financialDocument.SetCommission(commission);
            }
            else
            {
                //only reverse
                await UpdateFinancialDocument(context, financialDocument);
            }

            await unitOfWork.SaveChangesAsync();
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
                context.Message.PaymentGatewayType.HasValue ? (PaymentGatewayType)context.Message.PaymentGatewayType : null,
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
    }
}