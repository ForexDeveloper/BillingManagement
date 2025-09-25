using System;
using System.Linq;
using MassTransit;
using Domain.Core.Enums;
using System.Diagnostics;
using Shared.EventBus.Events;
using System.Threading.Tasks;
using Application.Service.Helper;
using System.Collections.Generic;
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
    IFinancialDocumentRepository financialDocumentRepository,
    IMerchantInstallmentRepository merchantInstallmentRepository,
    ITenantMerchantContractRepository tenantMerchantContractRepository,
    ILogger<FinancialDocumentAddedOrUpdatedEventConsumer> logger) : IConsumer<FcmFinancialDocumentAddedOrUpdatedEvent>
{
    public readonly ILogger<FinancialDocumentAddedOrUpdatedEventConsumer> _logger = logger;

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
                    financialDocument.ToBusinessIdentityId, financialDocument.TenantId);

                await CreateMerchantInstallments(contract, financialDocument);

                await unitOfWork.SaveChangesAsync();

                return;
            }

            //only reverse
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
                type,
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
            _logger.LogError(new LogStruct
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

    private async Task CreateMerchantInstallments(TenantMerchantContract contract, FinancialDocument financialDocument)
    {
        decimal commission = 0;

        List<MerchantInstallment> installments = [];

        var installmentDates = DateHelper.CalculateInstallments(DateTime.Now, contract.InstallmentsCount,
            TimeInterval.Day, contract.BillingBreak, contract.BillingPeriod, contract.BillingPeriodType);

        var installmentCount = contract.InstallmentsCount ?? 1;

        var installmentAmount = RoundHelper.RoundAmount(financialDocument.Amount / installmentCount);

        var lastInstallmentAmount = financialDocument.Amount - (installmentAmount * (installmentCount - 1));

        const decimal currentTransactionAmount = 15000000;

        decimal financialDocumentTargetAmount = 0;

        //if (!contract.CommissionReferenceTypes.Any())
        //{
        //    financialDocumentTargetAmount = financialDocument.Amount;
        //}
        //else
        //{
        //    foreach (var contractCommissionReferenceType in contract.CommissionReferenceTypes)
        //    {
        //        switch (contractCommissionReferenceType)
        //        {
        //            case CommissionReferenceType.PrepaymentAmount:
        //                financialDocumentTargetAmount += financialDocument.FinancialDocumentPayments
        //                    .Where(p => p.Type == FinancialDocumentPaymentType.Prepayment).Sum(p => p.Amount);
        //                break;

        //            case CommissionReferenceType.CashAmount:
        //                financialDocumentTargetAmount += financialDocument.FinancialDocumentPayments
        //                    .Where(p => p.Type == FinancialDocumentPaymentType.Cash).Sum(p => p.Amount);
        //                break;

        //            case CommissionReferenceType.CreditAmount:
        //                financialDocumentTargetAmount += financialDocument.FinancialDocumentPayments
        //                    .Where(p => p.Type == FinancialDocumentPaymentType.Credit).Sum(p => p.Amount);
        //                break;

        //            case CommissionReferenceType.InterestAmount:
        //                break;

        //            default:
        //                throw new ArgumentOutOfRangeException();
        //        }
        //    }
        //}

        switch (contract.CommissionCalculationType)
        {
            case CommissionCalculationType.UniformTiered:
                break;

            case CommissionCalculationType.CumulativeTiered:

                var tieredCommission = contract.TieredCommissions
                    .FirstOrDefault(p => currentTransactionAmount <= p.FromAmount ||
                                         (p.FromAmount <= currentTransactionAmount && currentTransactionAmount < p.ToAmount) ||
                                         p.ToAmount < currentTransactionAmount);

                if (tieredCommission == null) break;

                commission = financialDocumentTargetAmount * tieredCommission.Percentage;

                if (commission > tieredCommission.MaxAmount)
                {
                    commission = tieredCommission.MaxAmount.Value;
                }

                if (commission < tieredCommission.MinAmount)
                {
                    commission = tieredCommission.MinAmount.Value;
                }

                break;

            case CommissionCalculationType.FixedPercentage:

                commission = contract.FixedPercentageCommission.HasValue
                    ? financialDocumentTargetAmount * contract.FixedPercentageCommission.Value
                    : 0;

                if (commission > contract.TransactionMaxCommissionAmount)
                {
                    commission = contract.TransactionMaxCommissionAmount.Value;
                }

                if (commission < contract.TransactionMinCommissionAmount)
                {
                    commission = contract.TransactionMinCommissionAmount.Value;
                }

                break;

            case CommissionCalculationType.FixedAmount:

                commission = contract.FixedAmountCommission ?? 0;

                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        financialDocument.SetCommission(commission);

        var installmentCommission = RoundHelper.RoundAmount(commission / installmentCount);

        var lastInstallmentCommission = commission - (installmentCommission * (installmentCount - 1));

        for (var i = 0; i < installmentDates.Count; i++)
        {
            var installmentDate = installmentDates[i];

            var amount = i == installmentDates.Count - 1 ? lastInstallmentAmount : installmentAmount;

            var installment = new MerchantInstallment(financialDocument, financialDocument.TenantId,
                financialDocument.TenantId, financialDocument.ToBusinessIdentityId, contract.Id, amount, i + 1,
                installmentDate, InstallmentType.Installment);

            installments.Add(installment);

            if (contract.CommissionDeductionMethodType == CommissionDeductionMethodType.DeductFromFirstInstallment)
            {
                installments[0].SetCommission(commission);
            }
            else
            {
                installment.SetCommission(i == installmentDates.Count - 1
                    ? lastInstallmentCommission
                    : installmentCommission);
            }
        }

        await merchantInstallmentRepository.AddRangeAsync(installments);
    }
}