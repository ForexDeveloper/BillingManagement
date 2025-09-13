using System.ComponentModel;

namespace Domain.Core.Enums
{
    public enum WalletContractEndDateType : byte
    {
        [Description("کمتر 1 از روز")]
        LessThanOneDay = 1,

        [Description("کمتر از 2 هفته")]
        LessThanTwoWeeks = 2,

        [Description("کمتر از 1 ماه")]
        LessThanOneMonth = 3,

        [Description("کمتر از 6 ماه")]
        LessThanSixMonths = 4
    }
}
