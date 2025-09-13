using System.ComponentModel;

namespace Domain.Core.Enums;

public enum TransactionFeeType : byte
{
    [Description("درصدی ثابت از مبلغ هر تراکنش")]
    FixedPercentage = 1,

    [Description("مبلغی ثابت بابت هر تراکنش")]
    FixedAmount = 2
}