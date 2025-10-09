using System.ComponentModel;

namespace Domain.Core.Enums;

public enum TimeInterval : byte
{
    [Description("روزانه")]
    Day = 1,

    [Description("هفتگی")]
    Week = 2,

    [Description("ماهانه")]
    Month = 3
}