using System.ComponentModel;

namespace Domain.Core.Enums;

public enum ProviderOwner : byte
{
    [Description("کیپا")]
    Keepa = 1,
    [Description("مالک زیر ساخت")]
    Tenant = 2,
}