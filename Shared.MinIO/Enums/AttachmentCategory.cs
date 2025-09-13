using Shared.MinIO.Attributes;
using System.ComponentModel;

namespace Shared.MinIO.Enums
{
    public enum AttachmentCategory
    {
        [Description("روی کارت ملی")]
        [Sensitive] CartMeliFront = 1,
        [Description("پشت کارت ملی")]
        [Sensitive] CartMeliBack = 2,
        [Description("روزنامه رسمی")]
        [Sensitive] OfficialNewspaper = 3,
        [Description("جواز کسب")]
        [Sensitive] BusinessLicense = 4,
        [Description("سند تک برگ")]
        [Sensitive] PropertyDeed = 5,
        [Description("اجاره نامه")]
        [Sensitive] LeaseAgreement = 6,
        [Description("لوگو پلن")]
        PlanLogo = 9,
    }
}
