using System.ComponentModel;

namespace Domain.Core.Enums;

public enum InstallmentStatus : byte
{
    None=0,
    [Description("در انتظار پرداخت")]
    Pending = 1,

    [Description("پرداخت شده")]
    CompletePaid = 2,

    [Description("پرداخت ناقص")]
    PartiallyPaid = 3,

    [Description("معوق")]
    Overdue = 4
}
