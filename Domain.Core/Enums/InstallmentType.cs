using System.ComponentModel;

namespace Domain.Core.Enums;

public enum InstallmentType : byte
{
    [Description("قسط")]
    Installment = 1,

    [Description("جریمه")]
    Penalty = 2,

    [Description("کارمزد")]
    Commission = 3
}