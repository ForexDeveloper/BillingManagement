using System.ComponentModel;

namespace Domain.Core.Enums;

public enum InstallmentType : byte
{
    [Description("قسط")]
    Installment = 1,

    [Description("ریفاند")]
    Refund = 2,

    [Description("جریمه")]
    Penalty = 3,

    [Description("کارمزد")]
    Commission = 4
}