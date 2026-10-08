using System;
using Domain.Core.Enums;
using Domain.Core.Helper;
using Domain.Core.Constants;
using System.Collections.Generic;
using Domain.Core.Entities.Shared;
using Domain.Core.Entities.BillingAggregate;
using Domain.Core.Entities.Shared.Exceptions;

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
        TimeInterval periodType, DateTime startDate, DateTime endDate, int gracePeriod, int mainContractId,
        List<int> contractIds, decimal previousDebitAmount, decimal previousCreditAmount, decimal previousPenaltyAmount,
        decimal purchaseTransactionsAmount, decimal refundedTransactionsAmount, decimal purchaseTransactionsCommission,
        decimal refundedTransactionsCommission, decimal purchaseTransactionsCalculatedCommission,
        decimal tieredTransactionsAmount, List<TieredCalculatedLevel> tieredCalculatedLevels, Billing? pair = null,
        Billing? debtor = null, Billing? creditor = null) : base(tenantId, fromBusinessIdentityId, toBusinessIdentityId,
        type, periodType, previousDebitAmount, previousCreditAmount, previousPenaltyAmount, startDate, endDate,
        gracePeriod, mainContractId, contractIds, tieredTransactionsAmount, tieredCalculatedLevels, pair, debtor, creditor)
    {
        PurchaseTransactionsAmount = purchaseTransactionsAmount;
        RefundedTransactionsAmount = refundedTransactionsAmount;
        PurchaseTransactionsCommission = purchaseTransactionsCommission;
        RefundedTransactionsCommission = refundedTransactionsCommission;
        PurchaseTransactionsCalculatedCommission = purchaseTransactionsCalculatedCommission;
        Configure();
    }

    public bool IsAbsoluteZero()
    {
        if (Type == BillingType.TenantToMerchant)
        {
            return Amount == 0 &&
                   PreviousDebitAmount == 0 &&
                   PreviousCreditAmount == 0 &&
                   PurchaseTransactionsAmount == 0 &&
                   RefundedTransactionsAmount == 0 &&
                   PurchaseTransactionsCommission == 0 &&
                   RefundedTransactionsCommission == 0;
        }

        return Amount == 0 &&
               PreviousDebitAmount == 0 &&
               PurchaseTransactionsCommission == 0;
    }

    protected override void CalculateAmount()
    {
        if (Type == BillingType.TenantToMerchant)
        {
            var totalDebit = PreviousDebitAmount + PurchaseTransactionsAmount + RefundedTransactionsCommission + AdditionsAmount;

            var totalCredit = PreviousCreditAmount + PurchaseTransactionsCommission + RefundedTransactionsAmount + DeductionsAmount;

            Amount = totalDebit - totalCredit;
        }

        else if (Type == BillingType.MerchantToTenant)
        {
            Amount = PreviousDebitAmount + PurchaseTransactionsCommission;
        }
    }

    protected override string GenerateCodePrefix()
    {
        switch (Type)
        {
            case BillingType.TenantToMerchant:
                return BillingConstants.TenantPrefix;

            case BillingType.MerchantToTenant:
                return BillingConstants.MerchantPrefix;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void SetCheckSum()
    {
        CheckSum = GenerateCheckSum().Hash();
    }

    protected override void ValidateCheckSum()
    {
        var comperedTo = GenerateCheckSum();

        if (!comperedTo.Validate(CheckSum))
        {
            throw new ArgumentValidationException("InconsistentData", "اطلاعات موجود در دیتابیس صحیح نمی باشد.");
        }
    }

    private string GenerateCheckSum()
    {
        return $"{FromBusinessIdentityId}{ToBusinessIdentityId}{TenantId}{Amount:F10}" +
               $"{PreviousDebitAmount:F10}{PurchaseTransactionsAmount:F10}{RefundedTransactionsCommission:F10}{AdditionsAmount:F10}" +
               $"{PreviousCreditAmount:F10}{PurchaseTransactionsCommission:F10}{RefundedTransactionsAmount:F10}{DeductionsAmount:F10}" +
               $"{GracePeriod}{Status}{Transferred}{StartDate:yyyy-MM-ddTHH:mm:ss}{DueDate:yyyy-MM-ddTHH:mm:ss}{CreatedDateTime:yyyy-MM-ddTHH:mm:ss}";
    }
}