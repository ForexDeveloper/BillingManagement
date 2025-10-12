using System;
using MassTransit;
using System.Linq;
using Domain.Core.Enums;
using System.Diagnostics;
using Shared.EventBus.Events;
using System.Threading.Tasks;
using System.Collections.Generic;
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
    IFinancialDocumentRepository financialDocumentRepository,
    IMerchantInstallmentRepository merchantInstallmentRepository,
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

                //var contract = await tenantMerchantContractRepository.GetActiveContractAsync(
                //    financialDocument.ToBusinessIdentityId, financialDocument.TenantId);

                //if (financialDocument.Type == FinancialDocumentType.Purchase)
                //{
                //    var commission = await CreateMerchantInstallments(contract, financialDocument);

                //    financialDocument.SetCommission(commission);
                //}

                await unitOfWork.SaveChangesAsync();

                return;
            }

            //only reverse
            await UpdateFinancialDocument(context, financialDocument);
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

    private async Task<decimal> CreateMerchantInstallments(TenantMerchantContract contract, FinancialDocument financialDocument)
    {
        var today = DateTime.Today;

        List<MerchantInstallment> installments = [];

        var installmentDates = DateHelper.CalculateInstallments(today, contract.InstallmentsCount,
            TimeInterval.Day, contract.BillingBreak, contract.BillingPeriod, contract.BillingPeriodType);

        var financialDocumentTargetAmount = financialDocument.Amount;

        if (contract.CommissionReferenceTypes != null && contract.CommissionReferenceTypes.Any())
        {
            financialDocumentTargetAmount = 0;

            foreach (var contractCommissionReferenceType in contract.CommissionReferenceTypes)
            {
                switch (contractCommissionReferenceType)
                {
                    case CommissionReferenceType.CashAmount:
                        financialDocumentTargetAmount += financialDocument.CashAmount;
                        break;

                    case CommissionReferenceType.CreditAmount:
                        financialDocumentTargetAmount += financialDocument.CreditAmount;
                        break;

                    case CommissionReferenceType.PrepaymentAmount:
                        financialDocumentTargetAmount += financialDocument.PrepaymentAmount;
                        break;

                    case CommissionReferenceType.InterestAmount:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        decimal financialDocumentCommission = 0;

        switch (contract.CommissionCalculationType)
        {
            case CommissionCalculationType.UniformTiered:
                break;

            case CommissionCalculationType.CumulativeTiered:

                var (startOfPeriod, _) = ContractPeriodHelper.GetPeriodBySpecificDate(contract.BillingPeriod,
                    contract.BillingPeriodType, contract.DailyBillingOriginDate, today);

                var sumOfTieredTransactions = await merchantInstallmentRepository.GetSumOfTieredTransactionsInSpecificPeriod(contract,
                        startOfPeriod, today);

                var tieredCommission = contract.TieredCommissions.FirstOrDefault(p =>
                    p.FromAmount < sumOfTieredTransactions && sumOfTieredTransactions <= p.ToAmount);

                if (tieredCommission == null)
                {
                    var minTieredCommission = contract.TieredCommissions.MinBy(p => p.ToAmount);

                    var maxTieredCommission = contract.TieredCommissions.MaxBy(p => p.ToAmount);

                    if (sumOfTieredTransactions <= minTieredCommission.FromAmount)
                    {
                        tieredCommission = minTieredCommission;
                    }

                    else if (sumOfTieredTransactions > maxTieredCommission.ToAmount)
                    {
                        tieredCommission = maxTieredCommission;
                    }

                    if (tieredCommission == null) break;

                    financialDocumentCommission = financialDocumentTargetAmount * (tieredCommission.Percentage / 100);

                    if (financialDocumentCommission > tieredCommission.MaxAmount)
                    {
                        financialDocumentCommission = tieredCommission.MaxAmount.Value;
                    }

                    if (financialDocumentCommission < tieredCommission.MinAmount)
                    {
                        financialDocumentCommission = tieredCommission.MinAmount.Value;
                    }
                }

                break;

            case CommissionCalculationType.FixedPercentage:

                if (!contract.FixedPercentageCommission.HasValue) break;

                financialDocumentCommission = financialDocumentTargetAmount * (contract.FixedPercentageCommission.Value / 100);

                if (financialDocumentCommission > contract.TransactionMaxCommissionAmount)
                {
                    financialDocumentCommission = contract.TransactionMaxCommissionAmount.Value;
                }

                if (financialDocumentCommission < contract.TransactionMinCommissionAmount)
                {
                    financialDocumentCommission = contract.TransactionMinCommissionAmount.Value;
                }

                break;

            case CommissionCalculationType.FixedAmount:

                financialDocumentCommission = contract.FixedAmountCommission ?? 0;

                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        var installmentCount = contract.InstallmentsCount ?? 1;

        var installmentAmount = RoundHelper.RoundAmount(financialDocument.Amount / installmentCount);
        var lastInstallmentAmount = financialDocument.Amount - (installmentAmount * (installmentCount - 1));

        var installmentCashAmount = RoundHelper.RoundAmount(financialDocument.CashAmount / installmentCount);
        var lastInstallmentCashAmount = financialDocument.CashAmount - (installmentCashAmount * (installmentCount - 1));

        var installmentCreditAmount = RoundHelper.RoundAmount(financialDocument.CreditAmount / installmentCount);
        var lastInstallmentCreditAmount = financialDocument.CreditAmount - (installmentCreditAmount * (installmentCount - 1));

        var installmentPrepaymentAmount = RoundHelper.RoundAmount(financialDocument.PrepaymentAmount / installmentCount);
        var lastInstallmentPrepaymentAmount = financialDocument.PrepaymentAmount - (installmentPrepaymentAmount * (installmentCount - 1));

        var installmentCommission = RoundHelper.RoundAmount(financialDocumentCommission / installmentCount);
        var lastInstallmentCommission = financialDocumentCommission - (installmentCommission * (installmentCount - 1));

        for (var i = 0; i < installmentDates.Count; i++)
        {
            decimal amount;
            decimal cashAmount;
            decimal creditAmount;
            decimal prePaymentAmount;
            decimal commission;

            if (i == installmentDates.Count - 1)
            {
                amount = lastInstallmentAmount;
                cashAmount = lastInstallmentCashAmount;
                creditAmount = lastInstallmentCreditAmount;
                prePaymentAmount = lastInstallmentPrepaymentAmount;
                commission = lastInstallmentCommission;
            }
            else
            {
                amount = installmentAmount;
                cashAmount = installmentCashAmount;
                creditAmount = installmentCreditAmount;
                prePaymentAmount = installmentPrepaymentAmount;
                commission = installmentCommission;
            }

            var installmentDate = installmentDates[i];

            var installment = new MerchantInstallment(financialDocument, contract.TenantId,
                contract.TenantId, contract.MerchantId, contract.Id, amount, cashAmount,
                creditAmount, prePaymentAmount, i + 1, installmentDate, InstallmentType.Purchase);

            installments.Add(installment);

            switch (contract.CommissionDeductionMethodType)
            {
                case null:
                case CommissionDeductionMethodType.DeductEquallyFromInstallments:
                    installment.SetCommission(commission);
                    break;

                case CommissionDeductionMethodType.DeductFromFirstInstallment:
                    installments[0].SetCommission(financialDocumentCommission);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        await merchantInstallmentRepository.AddRangeAsync(installments);

        return financialDocumentCommission;
    }
}