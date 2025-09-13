using System.ComponentModel;

namespace Domain.Core.Enums;

public enum InstallmentState : byte
{
    None=0,
    [Description("در انتظار پرداخت")]
    Pending = 1,

    [Description("پرداخت شده")]
    CompletePaid = 2,

    [Description("پرداخت ناقص")]
    PartiallyPaid = 3, //Billing

    [Description("معوق")]
    Overdue = 4,  //Billing
}
