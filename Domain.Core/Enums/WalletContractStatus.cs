using System.ComponentModel;

namespace Domain.Core.Enums
{
    public enum WalletContractStatus : byte
    {
        [Description("در حال بررسی")]
        Reviewing = 1,
        [Description("رد")]
        Reject = 2,
        [Description("فعال")]
        Active = 3,
        [Description("غیر فعال")]
        DeActive = 4
    }
}
