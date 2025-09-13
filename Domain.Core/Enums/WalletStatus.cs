using System.ComponentModel;

namespace Domain.Core.Enums
{
    public enum WalletStatus : byte
    {
        [Description("فعال")]
        Active = 1,
        [Description("غیرفعال")]
        Deactive = 2,
        [Description("مسدود")]
        Suspend = 3
    }
}
