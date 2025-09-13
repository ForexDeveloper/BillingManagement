using System.ComponentModel;

namespace Domain.Core.Enums;

public enum BillingState : byte
{
    None = 0,

    [Description("در انتظار پرداخت")]
    Pending = 1,

    [Description("پرداخت ناقص")]
    PartiallyPaid = 2,

    [Description("پرداخت شده")]
    CompletePaid = 3,

    [Description("معوق")]
    Overdue = 4
}
