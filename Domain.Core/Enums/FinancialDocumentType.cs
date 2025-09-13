using System.ComponentModel;

namespace Domain.Core.Enums;

public enum FinancialDocumentType : byte
{
    [Description("خرید")]
    Purchase = 1,

    [Description("شارژ کیف پول")]
    WalletCharge = 2,

    [Description("پرداخت صورتحساب")]
    Billing = 3,

    [Description("پرداخت هزینه عملیات")]
    OperationalFee = 4,

    [Description("پرداخت هزینه اعتبارسنجی")]
    VerificationFee = 5,

    [Description("عودت وجه")]
    Refund = 6,

    [Description("برداشت از کیف پول نقدی")]
    WithdrawCashOut = 7
}
