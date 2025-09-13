using System.ComponentModel;

namespace Domain.Core.Enums
{
    public enum AccountStatus:byte
    {
        [Description("فعال")]
        Active = 1,
        [Description("غیرفعال")]
        Deactive = 2,
        [Description("معلق")]
        Suspend = 3
    }
}
