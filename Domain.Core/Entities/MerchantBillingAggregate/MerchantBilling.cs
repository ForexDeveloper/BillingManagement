using Domain.Core.Entities.BillingAggregate;
using Domain.Core.Entities.BillingPaymentAggregate;
using Domain.Core.Entities.InstallmentAggregate;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Enums;
using Domain.Core.Helper;
using System;
using System.Collections.Generic;

namespace Domain.Core.Entities.MerchantBillingAggregate;

public sealed class MerchantBilling : Billing
{
    public decimal Additions { get; private set; }

    public string? AdditionsDescription { get; private set; }

    public decimal Deductions { get; private set; }

    public string? DeductionsDescription { get; private set; }

    public decimal RefundedTransactionsCommission { get; private set; }

    public decimal CurrentPeriodFinalCommission { get; private set; }

    public decimal CurrentPeriodCalculatedCommission { get; private set; }

    public decimal CurrentPeriodPurchaseTransactions { get; private set; }

    public decimal PreviousPeriodRefundedTransactions { get; private set; }

    private MerchantBilling()
    {

    }

    public MerchantBilling(int tenantId, int fromBusinessIdentityId, int toBusinessIdentityId, BillingType type,
        TimeInterval periodType, decimal previousDebitAmount, decimal previousCreditAmount,
        decimal previousPenaltyAmount, DateTime startDate, DateTime endDate, int gracePeriod,
        IEnumerable<int> contractIds, decimal currentPeriodCalculatedCommission, decimal currentPeriodFinalCommission,
        decimal refundedTransactionsCommission, decimal previousPeriodRefundedTransactions,
        decimal currentPeriodPurchaseTransactions,
        Billing? parent = null) : base(tenantId, fromBusinessIdentityId, toBusinessIdentityId, type, periodType,
        previousDebitAmount, previousCreditAmount, previousPenaltyAmount, startDate, endDate, gracePeriod, contractIds,
        parent)
    {
        Additions = 0;
        Deductions = 0;
        CurrentPeriodFinalCommission = currentPeriodFinalCommission;
        RefundedTransactionsCommission = refundedTransactionsCommission;
        CurrentPeriodPurchaseTransactions = currentPeriodPurchaseTransactions;
        CurrentPeriodCalculatedCommission = currentPeriodCalculatedCommission;
        PreviousPeriodRefundedTransactions = previousPeriodRefundedTransactions;
        CalculateAmount();
        SettleOrIssue();
        SetCheckSum();
    }

    public MerchantBilling(int tenantId, int fromBusinessIdentityId, int toBusinessIdentityId, BillingType type,
        TimeInterval periodType, decimal previousDebitAmount, decimal previousCreditAmount,
        decimal previousPenaltyAmount, DateTime startDate, DateTime endDate, int gracePeriod,
        IEnumerable<int> contractIds, decimal currentPeriodCalculatedCommission, decimal currentPeriodFinalCommission,
        decimal refundedTransactionsCommission, decimal previousPeriodRefundedTransactions,
        decimal currentPeriodPurchaseTransactions, IEnumerable<Installment>? installments = null,
        Billing? parent = null) : base(tenantId,
        fromBusinessIdentityId, toBusinessIdentityId, type, periodType, previousDebitAmount, previousCreditAmount,
        previousPenaltyAmount, startDate, endDate, gracePeriod, contractIds, installments, parent)
    {
        Additions = 0;
        Deductions = 0;
        CurrentPeriodFinalCommission = currentPeriodFinalCommission;
        RefundedTransactionsCommission = refundedTransactionsCommission;
        CurrentPeriodPurchaseTransactions = currentPeriodPurchaseTransactions;
        CurrentPeriodCalculatedCommission = currentPeriodCalculatedCommission;
        PreviousPeriodRefundedTransactions = previousPeriodRefundedTransactions;
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

        Additions = additions;
        AdditionsDescription = additionDescription;
        CalculateAmount();
    }

    public void SetDeductions(decimal deductions, string? deductionDescription)
    {
        if (deductions < 0)
        {
            throw new ArgumentValidationException(nameof(deductions), "مبلغ کسورات نمی تواند از 0 کوچکتر باشد");
        }

        Amount += Deductions;

        if (Amount < deductions)
        {
            throw new ArgumentValidationException(nameof(deductions), "مبلغ کسورات نمی تواند از مبلغ کل صورتحساب بیشتر باشد");
        }

        DeductionsDescription = deductionDescription;
        Deductions = deductions;
        CalculateAmount();
    }

    private void CalculateAmount()
    {
        var totalCredit = PreviousDebitAmount + CurrentPeriodPurchaseTransactions + RefundedTransactionsCommission + Additions;

        var totalDebit = PreviousCreditAmount + CurrentPeriodFinalCommission + PreviousPeriodRefundedTransactions + Deductions;

        Amount = totalCredit - totalDebit;

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

    public void AddBillingPayment(long billingId, long paymentId, decimal amount, DateTime paymentDate)
    {
        Payments.Add(new BillingPayment(billingId, paymentId, amount, paymentDate));
    }
}