using System.ComponentModel;

namespace Domain.Core.Enums;

public enum FinancialMetric : byte
{
    [Description("مبلغ")]
    Absolute = 1,
    [Description("درصد")]
    Percent = 2,
}