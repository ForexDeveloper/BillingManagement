using System;
using MassTransit;
using Domain.Core.Enums;
using System.Diagnostics;
using Shared.EventBus.Events;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Application.Service.Contracts;
using Domain.Core.UnitOfWorkContracts;
using Shared.Logging.Abstraction.Models;
using Shared.Logging.Abstraction.Extensions;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate;

namespace Application.Service.EventConsumers;

public sealed class FinancialDocumentAddedOrUpdatedEventConsumer(
    IApplicationDbContextUnitOfWork unitOfWork,
    IMerchantInstallmentService merchantInstallmentService,
    IFinancialDocumentRepository financialDocumentRepository,
    ILogger<FinancialDocumentAddedOrUpdatedEventConsumer> logger,
    ITenantMerchantContractRepository tenantMerchantContractRepository) : IConsumer<FcmFinancialDocumentAddedOrUpdatedEvent>
{
    public async Task Consume(ConsumeContext<FcmFinancialDocumentAddedOrUpdatedEvent> context)
    {
        var succeed = true;
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        const string SERVICE_NAME = $"{nameof(FinancialDocumentAddedOrUpdatedEventConsumer)}_{nameof(Consume)}";
        try
        {
            var financialDocument = await financialDocumentRepository.GetByIdAsync(context.Message.Id);

            if (financialDocument == null)
            {
                //purchase and refund
                financialDocument = await CreateFinancialDocument(context);

                if (financialDocument.TenantMerchantContractId.HasValue)
                {
                    decimal commission;

                    var contractId = financialDocument.TenantMerchantContractId.Value;

                    var contract = await tenantMerchantContractRepository.GetAsync(contractId);

                    if (financialDocument.Type == FinancialDocumentType.Purchase)
                    {
                        commission = await merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);
                    }
                    else
                    {
                        commission = await merchantInstallmentService.CreateRefundInstallments(contract, financialDocument);
                    }

                    financialDocument.SetCommission(commission);
                }
            }
            else
            {
                //only reverse

                UpdateFinancialDocument(context, financialDocument);
            }

            await unitOfWork.SaveChangesAsync();
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

    private void UpdateFinancialDocument(ConsumeContext<FcmFinancialDocumentAddedOrUpdatedEvent> context, FinancialDocument financialDocument)
    {
        //1 => purchase
        if (context.Message.Type != 1)
        {
            var exception = new ArgumentValidationException(nameof(context.Message.Type), "نوع سند مالی برای بازگشت کامل وجه خرید معتبر نیست.");

            logger.LogError(new LogStruct
            {
                Exception = exception,
                Results = string.Empty,
                Message = exception.Message,
                Tags = LogMessageTag.EventBus,
                InputParams = context.Message,
                ServiceName = $"{nameof(FinancialDocumentAddedOrUpdatedEventConsumer)}_{nameof(Consume)}"
            });

            throw exception;
        }

        financialDocument.SetState(FinancialDocumentState.Reverse);

        financialDocumentRepository.Update(financialDocument);
    }
}