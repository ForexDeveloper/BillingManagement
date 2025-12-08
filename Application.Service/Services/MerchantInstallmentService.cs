using Application.Service.Contracts;
using Application.Service.Helper;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.MerchantInstallmentAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate;
using Domain.Core.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Service.Services;

public sealed class MerchantInstallmentService(IMerchantInstallmentRepository merchantInstallmentRepository,
    IFinancialDocumentService financialDocumentService) : IMerchantInstallmentService
{
    public async Task<decimal> CreatePurchaseInstallments(TenantMerchantContract contract, FinancialDocument financialDocument)
    {
        var financialDocumentTargetAmount = financialDocument.Amount;

        if (contract.CommissionReferenceTypes != null && contract.CommissionReferenceTypes.Count != 0)
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

        decimal purchaseCommission = 0;

        switch (contract.CommissionCalculationType)
        {
            case CommissionCalculationType.FixedAmount:

                purchaseCommission = contract.FixedAmountCommission ?? 0;

                break;

            case CommissionCalculationType.FixedPercentage:

                if (!contract.FixedPercentageCommission.HasValue) break;

                purchaseCommission = CalculateFixedPercentageCommission(contract, financialDocumentTargetAmount);

                break;

            case CommissionCalculationType.UniformTiered:
            case CommissionCalculationType.CumulativeTiered:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        var installmentCount = contract.InstallmentsCount;

        var installmentAmount = RoundHelper.RoundAmount(financialDocument.Amount / installmentCount);
        var lastInstallmentAmount = financialDocument.Amount - (installmentAmount * (installmentCount - 1));

        var installmentCashAmount = RoundHelper.RoundAmount(financialDocument.CashAmount / installmentCount);
        var lastInstallmentCashAmount = financialDocument.CashAmount - (installmentCashAmount * (installmentCount - 1));

        var installmentCreditAmount = RoundHelper.RoundAmount(financialDocument.CreditAmount / installmentCount);
        var lastInstallmentCreditAmount = financialDocument.CreditAmount - (installmentCreditAmount * (installmentCount - 1));

        var installmentPrepaymentAmount = RoundHelper.RoundAmount(financialDocument.PrepaymentAmount / installmentCount);
        var lastInstallmentPrepaymentAmount = financialDocument.PrepaymentAmount - (installmentPrepaymentAmount * (installmentCount - 1));

        var installmentCommission = RoundHelper.RoundAmount(purchaseCommission / installmentCount);
        var lastInstallmentCommission = purchaseCommission - (installmentCommission * (installmentCount - 1));

        var installmentDates = DateHelper.CalculateInstallmentDates(financialDocument.CreatedDateTime, installmentCount,
            TimeInterval.Day, contract.BillingBreak, contract.BillingPeriod, contract.BillingPeriodType);

        List<MerchantInstallment> installments = [];

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

            var installment = new MerchantInstallment(financialDocument, financialDocument.TenantId,
                financialDocument.TenantId, financialDocument.ToBusinessIdentityId, contract.Id, amount, cashAmount,
                creditAmount, prePaymentAmount, i + 1, installmentDate, InstallmentType.Purchase);

            installments.Add(installment);

            switch (contract.CommissionDeductionMethodType)
            {
                case null:
                case CommissionDeductionMethodType.DeductEquallyFromInstallments:
                    installment.SetCommission(commission);
                    break;

                case CommissionDeductionMethodType.DeductFromFirstInstallment:
                    installments[0].SetCommission(purchaseCommission);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        await merchantInstallmentRepository.AddRangeAsync(installments);

        return purchaseCommission;
    }

    public async Task<decimal> CreateRefundInstallments(TenantMerchantContract contract, FinancialDocument financialDocument)
    {
        decimal refundCommission = 0;

        if (contract.CommissionCalculationType is CommissionCalculationType.FixedAmount or CommissionCalculationType.FixedPercentage)
        {
            refundCommission = await financialDocumentService.CalculateRefundCommission(financialDocument);
        }

        var installmentCount = contract.InstallmentsCount;

        var installmentAmount = RoundHelper.RoundAmount(financialDocument.Amount / installmentCount);
        var lastInstallmentAmount = financialDocument.Amount - (installmentAmount * (installmentCount - 1));

        var installmentCashAmount = RoundHelper.RoundAmount(financialDocument.CashAmount / installmentCount);
        var lastInstallmentCashAmount = financialDocument.CashAmount - (installmentCashAmount * (installmentCount - 1));

        var installmentCreditAmount = RoundHelper.RoundAmount(financialDocument.CreditAmount / installmentCount);
        var lastInstallmentCreditAmount = financialDocument.CreditAmount - (installmentCreditAmount * (installmentCount - 1));

        var installmentPrepaymentAmount = RoundHelper.RoundAmount(financialDocument.PrepaymentAmount / installmentCount);
        var lastInstallmentPrepaymentAmount = financialDocument.PrepaymentAmount - (installmentPrepaymentAmount * (installmentCount - 1));

        var installmentCommission = RoundHelper.RoundAmount(refundCommission / installmentCount);
        var lastInstallmentCommission = refundCommission - (installmentCommission * (installmentCount - 1));

        var installmentDates = DateHelper.CalculateInstallmentDates(financialDocument.CreatedDateTime, installmentCount,
            TimeInterval.Day, 0, contract.BillingPeriod, contract.BillingPeriodType);

        List<MerchantInstallment> installments = [];

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

            var installment = new MerchantInstallment(financialDocument, financialDocument.TenantId,
                financialDocument.TenantId, financialDocument.ToBusinessIdentityId, contract.Id, amount, cashAmount,
                creditAmount, prePaymentAmount, i + 1, installmentDate, InstallmentType.Refund);

            installments.Add(installment);

            switch (contract.CommissionDeductionMethodType)
            {
                case null:
                case CommissionDeductionMethodType.DeductEquallyFromInstallments:
                    installment.SetCommission(commission);
                    break;

                case CommissionDeductionMethodType.DeductFromFirstInstallment:
                    installments[0].SetCommission(refundCommission);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        await merchantInstallmentRepository.AddRangeAsync(installments);

        return refundCommission;
    }

    private static decimal CalculateFixedPercentageCommission(TenantMerchantContract contract, decimal financialDocumentTargetAmount)
    {
        if (!contract.FixedPercentageCommission.HasValue) return 0;

        var financialDocumentCommission = financialDocumentTargetAmount * (contract.FixedPercentageCommission.Value / 100);

        financialDocumentCommission = RoundHelper.RoundAmount(financialDocumentCommission);

        if (financialDocumentCommission > contract.TransactionMaxCommissionAmount)
        {
            financialDocumentCommission = contract.TransactionMaxCommissionAmount.Value;
        }

        if (financialDocumentCommission < contract.TransactionMinCommissionAmount)
        {
            financialDocumentCommission = contract.TransactionMinCommissionAmount.Value;
        }

        return financialDocumentCommission;
    }
}