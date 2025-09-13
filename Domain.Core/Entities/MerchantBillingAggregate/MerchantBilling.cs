using System;
using System.Linq;
using Domain.Core.Enums;
using Domain.Core.Helper;
using System.Collections.Generic;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.B2bBillingAggregate;
using Domain.Core.Entities.B2bInstallmentAggregate;

namespace Domain.Core.Entities.MerchantBillingAggregate;

public sealed class MerchantBilling : B2bBilling
{
    public decimal Additions { get; private set; }

    public decimal Deductions { get; private set; }

    public decimal RefundedPurchasesCommission { get; private set; }

    public decimal CurrentPeriodFinalCommission { get; private set; }

    public decimal CurrentPeriodPurchaseTransactions { get; private set; }

    public decimal PreviousPeriodRefundedPurchases { get; private set; }

    private MerchantBilling()
    {

    }

    public MerchantBilling(int tenantId, int fromBusinessIdentityId, int toBusinessIdentityId, BillingType type,
        decimal amount, decimal previousDebitAmount, decimal previousCreditAmount, decimal previousPenaltyAmount,
        DateTime startDate, DateTime endDate, int gracePeriod, decimal refundedPurchasesCommission,
        decimal currentPeriodFinalCommission, decimal previousPeriodRefundedPurchases,
        decimal currentPeriodPurchaseTransactions, B2bBilling? parent = null) : base(tenantId, fromBusinessIdentityId,
        toBusinessIdentityId, type, amount, previousDebitAmount, previousCreditAmount, previousPenaltyAmount, startDate,
        endDate, gracePeriod, parent)
    {
        Additions = 0;
        Deductions = 0;
        RefundedPurchasesCommission = refundedPurchasesCommission;
        CurrentPeriodFinalCommission = currentPeriodFinalCommission;
        PreviousPeriodRefundedPurchases = previousPeriodRefundedPurchases;
        CurrentPeriodPurchaseTransactions = currentPeriodPurchaseTransactions;
        SetCheckSum();
    }

    public MerchantBilling(int tenantId, int fromBusinessIdentityId, int toBusinessIdentityId, BillingType type,
        decimal amount, decimal previousDebitAmount, decimal previousCreditAmount, decimal previousPenaltyAmount,
        DateTime startDate, DateTime endDate, int gracePeriod, decimal refundedPurchasesCommission,
        decimal currentPeriodFinalCommission, decimal previousPeriodRefundedPurchases,
        decimal currentPeriodPurchaseTransactions, IEnumerable<B2bInstallment>? installments = null,
        B2bBilling? parent = null) : base(tenantId, fromBusinessIdentityId, toBusinessIdentityId, type, amount,
        previousDebitAmount, previousCreditAmount, previousPenaltyAmount, startDate, endDate, gracePeriod, installments,
        parent)
    {
        Additions = 0;
        Deductions = 0;
        RefundedPurchasesCommission = refundedPurchasesCommission;
        CurrentPeriodFinalCommission = currentPeriodFinalCommission;
        PreviousPeriodRefundedPurchases = previousPeriodRefundedPurchases;
        CurrentPeriodPurchaseTransactions = currentPeriodPurchaseTransactions;
        SetCheckSum();
    }

    public decimal CalculateDebitAmount()
    {
        return Amount + PreviousDebitAmount - Payments.Sum(p => p.Amount);
    }

    public void SetAdditions(decimal additions)
    {
        if (additions < 0)
        {
            throw new ArgumentValidationException(nameof(additions), "مبلغ اضافات نمی تواند از 0 کوچکتر باشد");
        }

        Additions = additions;
    }

    public void SetDeductions(decimal deductions)
    {
        if (deductions < 0)
        {
            throw new ArgumentValidationException(nameof(deductions), "مبلغ کسورات نمی تواند از 0 کوچکتر باشد");
        }

        Deductions = deductions;
    }

    protected override void SetCheckSum()
    {
        CheckSum = $"{FromBusinessIdentityId}{ToBusinessIdentityId}{TenantId}{Amount:F10}{PreviousDebitAmount:F10}{PreviousCreditAmount:F10}{PreviousPenaltyAmount:F10}{GracePeriod}{Status}{StartDate:yyyy-MM-ddTHH:mm:ss}{EndDate:yyyy-MM-ddTHH:mm:ss}{CreatedDateTime:yyyy-MM-ddTHH:mm:ss}".Hash();
    }

    protected override void ValidateCheckSum()
    {
        var comperedTo = $"{FromBusinessIdentityId}{ToBusinessIdentityId}{TenantId}{Amount:F10}{PreviousDebitAmount:F10}{PreviousCreditAmount:F10}{PreviousPenaltyAmount:F10}{GracePeriod}{Status}{StartDate:yyyy-MM-ddTHH:mm:ss}{EndDate:yyyy-MM-ddTHH:mm:ss}{CreatedDateTime:yyyy-MM-ddTHH:mm:ss}";

        if (!comperedTo.Validate(CheckSum))
        {
            throw new ArgumentValidationException("InconsistentData", "اطلاعات موجود در دیتابیس صحیح نمی باشد.");
        }
    }
}