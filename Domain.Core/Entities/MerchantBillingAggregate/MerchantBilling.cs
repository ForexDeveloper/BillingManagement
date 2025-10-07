using System;
using Domain.Core.Enums;
using Domain.Core.Helper;
using System.Collections.Generic;
using Domain.Core.Entities.BillingAggregate;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.InstallmentAggregate;

namespace Domain.Core.Entities.MerchantBillingAggregate;

public sealed class MerchantBilling : Billing
{
    public decimal PurchaseTransactionsAmount { get; private set; }

    public decimal RefundedTransactionsAmount { get; private set; }

    public decimal PurchaseTransactionsCommission { get; private set; }

    public decimal RefundedTransactionsCommission { get; private set; }

    public decimal PurchaseTransactionsCalculatedCommission { get; private set; }

    private MerchantBilling()
    {

    }

    public MerchantBilling(int tenantId, int fromBusinessIdentityId, int toBusinessIdentityId, BillingType type,
        TimeInterval periodType, DateTime startDate, DateTime endDate, int gracePeriod, List<int> contractIds,
        decimal previousDebitAmount, decimal previousCreditAmount, decimal previousPenaltyAmount,
        decimal purchaseTransactionsAmount, decimal refundedTransactionsAmount, decimal purchaseTransactionsCommission,
        decimal refundedTransactionsCommission, decimal purchaseTransactionsCalculatedCommission,
        Billing? debtor = null, Billing? creditor = null) : base(tenantId, fromBusinessIdentityId, toBusinessIdentityId,
        type, periodType, previousDebitAmount, previousCreditAmount, previousPenaltyAmount, startDate, endDate,
        gracePeriod, contractIds, debtor, creditor)
    {
        PurchaseTransactionsAmount = purchaseTransactionsAmount;
        RefundedTransactionsAmount = refundedTransactionsAmount;
        PurchaseTransactionsCommission = purchaseTransactionsCommission;
        RefundedTransactionsCommission = refundedTransactionsCommission;
        PurchaseTransactionsCalculatedCommission = purchaseTransactionsCalculatedCommission;
        CalculateAmount();
        SettleOrIssue();
        SetCheckSum();
    }

    public MerchantBilling(int tenantId, int fromBusinessIdentityId, int toBusinessIdentityId, BillingType type,
        TimeInterval periodType, DateTime startDate, DateTime endDate, int gracePeriod, List<int> contractIds,
        decimal previousDebitAmount, decimal previousCreditAmount, decimal previousPenaltyAmount,
        decimal purchaseTransactionsAmount, decimal refundedTransactionsAmount, decimal purchaseTransactionsCommission,
        decimal refundedTransactionsCommission, decimal purchaseTransactionsCalculatedCommission,
        IEnumerable<Installment>? installments = null, Billing? debtor = null, Billing? creditor = null) : base(
        tenantId, fromBusinessIdentityId, toBusinessIdentityId, type, periodType, previousDebitAmount,
        previousCreditAmount, previousPenaltyAmount, startDate, endDate, gracePeriod, contractIds, installments, debtor,
        creditor)
    {
        PurchaseTransactionsAmount = purchaseTransactionsAmount;
        RefundedTransactionsAmount = refundedTransactionsAmount;
        PurchaseTransactionsCommission = purchaseTransactionsCommission;
        RefundedTransactionsCommission = refundedTransactionsCommission;
        PurchaseTransactionsCalculatedCommission = purchaseTransactionsCalculatedCommission;
        CalculateAmount();
        SettleOrIssue();
        SetCheckSum();
    }

    public void SetAdditions(decimal additions, string? additionDescription)
    {
        if (additions < 0)
        {
            throw new ArgumentValidationException(nameof(additions), "مبلغ اضافات نمی تواند از 0 کوچکتر باشد");
        }

        AdditionsAmount = additions;
        AdditionsDescription = additionDescription;
        CalculateAmount();
    }

    public void SetDeductions(decimal deductions, string? deductionDescription)
    {
        if (deductions < 0)
        {
            throw new ArgumentValidationException(nameof(deductions), "مبلغ کسورات نمی تواند از 0 کوچکتر باشد");
        }

        Amount += DeductionsAmount;

        if (Amount < deductions)
        {
            throw new ArgumentValidationException(nameof(deductions), "مبلغ کسورات نمی تواند از مبلغ کل صورتحساب بیشتر باشد");
        }

        DeductionsDescription = deductionDescription;
        DeductionsAmount = deductions;
        CalculateAmount();
    }

    private void CalculateAmount()
    {
        var totalDebit = PreviousDebitAmount + PurchaseTransactionsAmount + RefundedTransactionsCommission + AdditionsAmount;

        var totalCredit = PreviousCreditAmount + PurchaseTransactionsCommission + RefundedTransactionsAmount + DeductionsAmount;

        Amount = totalDebit - totalCredit;

        SetCheckSum();
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