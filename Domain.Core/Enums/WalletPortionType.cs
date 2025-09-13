using System.ComponentModel;

namespace Domain.Core.Enums
{
    public enum WalletPortionType : byte
    {
        [Description("مازاد نقدی")]
        CashSurplus = 1,
        [Description("پیش پرداخت نقدی")]
        CashAdvance = 2,
        [Description("اعتباری")]
        Credit = 3,
        [Description("بهره مشتری")]
        Interest = 4
    }
}
