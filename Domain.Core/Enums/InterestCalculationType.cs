using System.ComponentModel;

namespace Domain.Core.Enums;

public enum InterestCalculationType : byte
{
    [Description("به صورت درصد")]
    PercentageOfPurchase = 1,

    [Description("به صورت مبلغ ثابت")]
    FixedAmountPerPurchase = 2
}