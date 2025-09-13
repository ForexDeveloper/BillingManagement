using Application.Query.Base;
using Domain.Core.Enums;

namespace Service.Rest.V1.RequestModels.IpgSettings;

public class GetTenantIpgSettingsModel : GetTenantIpgSettingsBaseModel
{
    public int? TenantId { get; set; }
}


public class GetTenantIpgSettingsBaseModel : BasePaginatedListRequest
{
    public IpgSettingOwnerType? IpgSettingOwnerType { get; set; }
    public bool? IsActive { get; set; }
}