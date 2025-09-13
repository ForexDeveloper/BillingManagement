using System.ComponentModel;

namespace Domain.Core.Enums;

public enum CashOutRequestStatus:byte
{
    [Description("در انتظار تایید")]
    AwaitingApproval = 1,

    [Description("پرداخت شده")]
    Paid = 2,

    [Description("رد شده")]
    Rejected = 3,
}