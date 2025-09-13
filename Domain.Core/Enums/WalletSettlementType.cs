using System.ComponentModel;

namespace Domain.Core.Enums
{
    public enum WalletSettlementType : byte
    {
        [Description("نقدی")]
        Cash = 1,

        [Description("چک تسویه")]
        Cheque = 2
    }
}
