using System.ComponentModel;

namespace Domain.Core.Enums
{
    public enum InterestPortionType : byte
    {
        [Description("نقدی")]
        CashAdvance = 1,

        [Description("اعتباری")]
        Credit = 2,

        [Description("بهره مشتری")]
        Interest = 3
    }
}
