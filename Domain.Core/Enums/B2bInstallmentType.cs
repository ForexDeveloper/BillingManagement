using System.ComponentModel;

namespace Domain.Core.Enums;

public enum B2bInstallmentType : byte
{
    [Description("قسط")]
    Installment = 1,

    [Description("جریمه")]
    Penalty = 2,

    [Description("کارمزد")]
    Commission = 3
}