using System.ComponentModel;

namespace Domain.Core.Enums;

public enum TransactionType : byte
{
    [Description("خرید")]
    Purchase = 1,

    [Description("شارژ")]
    Charge = 2,

    [Description("انتقال")]
    Transfer = 3,

    [Description("پرداخت")]
    Payment = 4,

    [Description("برگشت")]
    Reverse = 5,

    [Description("برداشت وجه")]
    Withdrawal = 6,

    [Description("عودت وجه")]
    Refund = 7,

    [Description("هزینه عملیات")]
    OperationalFee = 8,

    [Description("هزینه اعتبارسنجی")]
    VerificationFee = 9
}
