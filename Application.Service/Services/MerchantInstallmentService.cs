using System;
using System.Linq;
using Domain.Core.Enums;
using System.Threading.Tasks;
using Application.Service.Helper;
using System.Collections.Generic;
using Application.Service.Contracts;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.MerchantInstallmentAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate;

namespace Application.Service.Services;

public sealed class MerchantInstallmentService(IMerchantInstallmentRepository merchantInstallmentRepository) : IMerchantInstallmentService
{
    public async Task<decimal> CreateInstallments(TenantMerchantContract contract, FinancialDocument financialDocument)
    {
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
            case CommissionCalculationType.FixedAmount:

                financialDocumentCommission = contract.FixedAmountCommission ?? 0;

                break;

            case CommissionCalculationType.FixedPercentage:

                financialDocumentCommission = CalculateFixedPercentageCommission(contract, financialDocumentTargetAmount);

                break;

            case CommissionCalculationType.UniformTiered:
            case CommissionCalculationType.CumulativeTiered:
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

        var installmentDates = DateHelper.CalculateInstallments(financialDocument.CreatedDateTime, installmentCount,
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
                    installments[0].SetCommission(financialDocumentCommission);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        await merchantInstallmentRepository.AddRangeAsync(installments);

        return financialDocumentCommission;
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