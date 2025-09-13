using System.ComponentModel;

namespace Domain.Core.Enums;

public enum BillingPaymentState : byte
{
    [Description("در انتظار تایید")]
    Pending = 1, //todo
    [Description("پرداخت شده")]
    Paid = 2
}
