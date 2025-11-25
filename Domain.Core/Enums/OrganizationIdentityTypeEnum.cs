using System.ComponentModel;

namespace Domain.Core.Enums
{
    public enum OrganizationIdentityTypeEnum
    {
        [Description("شناس")]
        Identified = 1,
        [Description("ناشناس")]
        Anonymous = 2,
        [Description("هردو")]
        Mixed = 3
    }
}
