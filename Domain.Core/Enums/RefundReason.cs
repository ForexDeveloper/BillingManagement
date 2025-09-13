using System.ComponentModel;

namespace Domain.Core.Enums;

public enum RefundReason : byte
{
    [Description("خرید تکراری")]
    DuplicatePurchase = 1,

    [Description("درخواست مشتری")]
    CustomerRequest = 2,

    [Description("مشکوک به تقلب")]
    SuspectedFraud = 3,

    [Description("سایر")]
    Other = 4
}
