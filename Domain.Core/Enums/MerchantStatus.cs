using System.ComponentModel;

namespace Domain.Core.Enums
{
    public enum MerchantStatus : byte
    {
        [Description("در حال بررسی")]
        UnderReview = 1,
        [Description("غیر فعال")]
        Inactive = 2,
        [Description("فعال")]
        Active = 3,
        [Description("تعلیق شده")]
        Suspended = 4

    }
}
