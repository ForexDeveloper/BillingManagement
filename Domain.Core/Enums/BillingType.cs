using System.ComponentModel;

namespace Domain.Core.Enums;

public enum BillingType : byte
{
    [Description("صورتحساب سهم پذیرنده")]
    TenantToMerchant = 1,

    [Description("صورتحساب سهم بهره بردار")]
    MerchantToTenant = 2,

    [Description("صورتحساب سهم پلتفرم")]
    TenantToPlatform = 3
}