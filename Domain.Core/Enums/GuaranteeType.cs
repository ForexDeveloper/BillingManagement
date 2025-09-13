using System.ComponentModel;

namespace Domain.Core.Enums
{
    public enum GuaranteeType : byte
    {
        [Description("چک صیادی")]
        Cheque = 1,

        [Description("سفته")]
        PromissoryNote = 2,

        [Description("ضمانت‌نامه")]
        Warranty = 3,

        [Description("سند تک برگ")]
        House = 4,

        [Description("سایر")]
        Other = 5
    }
}
