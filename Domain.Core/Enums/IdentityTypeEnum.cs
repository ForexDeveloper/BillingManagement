using System.ComponentModel;

namespace Domain.Core.Enums;

public enum IdentityTypeEnum : byte
{
    [Description("حقوقی")]
    Legal = 1,
    [Description("حقیقی")]
    Individual = 2,
    [Description("ناشناس")]
    Anonymous = 3
}