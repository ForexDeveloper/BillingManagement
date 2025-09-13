using System.ComponentModel;

namespace Domain.Core.Enums;

public enum PlatformFeeMethodType : byte
{
    [Description("بابت هر تراکنش")]
    PerTransaction = 1,

    [Description("حق اشتراک")]
    Subscriptions = 2,

    [Description("بابت هر تراکنش و حق اشتراک")]
    Both = 3
}