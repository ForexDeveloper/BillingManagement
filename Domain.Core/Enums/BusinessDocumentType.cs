using System.ComponentModel;

namespace Domain.Core.Enums
{
    public enum BusinessDocumentType : byte
    {
        [Description("جواز کسب")]
        BusinessLicense = 1,

        [Description("سند تک برگ")]
        PropertyDeed = 2,

        [Description("اجاره نامه")]
        LeaseAgreement = 3
    }
}
