using Application.Query.Base;
using Domain.Core.Enums;

namespace Service.Rest.V1.RequestModels.IpgSettings;

public class GetTenantIpgPaymentSettingsModel : BasePaginatedListRequest
{
    public IpgSettingOwnerType? IpgPaymentOwnerType { get; set; }
    public bool? IsActive { get; set; }
}