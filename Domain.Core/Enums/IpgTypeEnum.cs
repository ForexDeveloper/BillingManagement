using System.ComponentModel;

namespace Domain.Core.Enums;

public enum IpgTypeEnum : byte
{
    [Description("سپ")]
    Sep = 1,
    [Description("سپهر")]
    Sepehr = 2,
    [Description("بازار پی")]
    BazaarPay = 3,
}
