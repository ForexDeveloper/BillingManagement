using System.ComponentModel;

namespace Domain.Core.Enums;

public enum BillingStatus : byte
{
    [Description("صادر شده")]
    Issued = 1,

    [Description("پرداخت شده جزئی")]
    PartiallyPaid = 2,

    [Description("تسویه ‌شده")]
    Settled = 3,

    [Description("معوق شده")]
    Overdue = 4
}