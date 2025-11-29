using System.ComponentModel;

namespace Domain.Core.Enums
{
    public enum OrganizationTypeEnum
    {
        [Description("کلاسیک")]
        Classic = 1,
        [Description("هم پیمان")]
        CoWallet = 2,
    }
}
