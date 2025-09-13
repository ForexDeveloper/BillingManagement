using System.ComponentModel;

namespace Domain.Core.Enums;

public enum InstallmentType : byte
{
    [Description("قسط")]
    Installment = 1,

    [Description("بهره")]
    Interest = 2,

    [Description("جریمه")]
    Penalty = 3,

    [Description("کارمزد")]
    Commission = 4,

    [Description("بخشودگی")]
    Waiver = 5
}
