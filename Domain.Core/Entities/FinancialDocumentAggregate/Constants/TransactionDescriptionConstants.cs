namespace Domain.Core.Entities.FinancialDocumentAggregate.Constants;

public static class TransactionDescriptionConstants
{
    public const string CHARGE_LOAN = "واریز مبلغ تسهیلات";
    public const string OPERATIONAL_FEE = "پرداخت هزینه عملیات";
    public const string VERIFICATION_FEE = "پرداخت هزینه اعتبارسنجی";
    public const string TRANSFER_TO_TENANT_PURCHASE = "انتقال به حساب خرید {0}";
    public const string PAYMENT_TO_TENANT_BANK = "واریز مبلغ به حساب بانک {0}";
    public const string PAYMENT_TO_CUSTOMER_BANK = "واریز مبلغ به حساب بانک";
    public const string CHARGE_TO_TENANT_PURCHASE = "واریز به حساب خرید {0}";
    public const string CHARGE_TO_TENANT_LOAN = "واریز به حساب تسویه تسهیلات {0}";
    public const string CHARGE_TO_TENANT_INTEREST = "واریز به حساب بهره {0}";
    public const string CHARGE_TO_TENANT_PENALTY = "واریز به حساب جریمه {0}";
    public const string CHARGE_CASH_WALLET = "شارژ کیف پول نقدی";
    public const string WITHDRAW_CASH_WALLET = "برداشت از کیف پول نقدی";
    public const string REVERSE = "بازگشت تراکنش";
    public const string REFUND = "عودت وجه تراکنش";
}
