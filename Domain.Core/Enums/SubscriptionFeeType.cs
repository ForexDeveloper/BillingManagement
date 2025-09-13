using System.ComponentModel;

namespace Domain.Core.Enums
{
    public enum SubscriptionFeeType : byte
    {
        [Description("بازپرداخت نقدی مبلغ کل")]
        FullUpfrontPayment = 1,

        [Description("بازپرداخت اقساطی")]
        InstallmentPayment = 2,

        [Description("بازپرداخت ترکیبی(تقدی و اقساطی)")]
        MixedPayment = 3
    }
}
