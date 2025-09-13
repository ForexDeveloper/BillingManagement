using System.ComponentModel;

namespace Domain.Core.Enums;

public enum InstallmentCategory : byte
{
    [Description("اقساط مشتری به مالک زیرساخت")]
    CustomerToTenant = 1,

    [Description("اقساط مالک زیرساخت به پذیرنده")]
    TenantToMerchant = 2,

    [Description("اقساط مالک زیرساخت به تسهیلگر")]
    TenantToFacilitator = 3,

    [Description("اقساط مالک زیرساخت به تامین کننده مالی")]
    TenantToFinancier = 4,

    [Description("اقساط مالک زیرساخت به ضامن")]
    TenantToGuarantor = 5,

    [Description("اقساط پذیرنده به مالک زیرساخت")]
    MerchantToTenant = 6,

    [Description("اقساط مالک زیرساخت به مالک زیرساخت")]
    TenantToTenant = 7
}
