using System.ComponentModel;

namespace Application.Service.Enums
{
    public enum WalletContractOriginType : byte
    {
        [Description("پنل ادمین")]
        PanelAdmin = 1,

        [Description("گرنتینگ")]
        Granting = 2,

        [Description("کش ولت")]
        CashWallet = 3

    }
}
