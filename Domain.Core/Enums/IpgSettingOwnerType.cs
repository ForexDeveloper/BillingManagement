using System.ComponentModel;

namespace Domain.Core.Enums
{
    public enum IpgSettingOwnerType : byte
    {
        [Description("پلتفرم")]
        Platform = 1,

        [Description("مالک زیر ساخت")]
        Tenant = 2
    }
}
