using Application.Query.Base;

namespace Service.Rest.V1.RequestModels.Organizations;

public class GetOrganizationsModel : GetOrganizationsBaseModel
{
    public int? TenantId { get; set; }
}

public class GetOrganizationsBaseModel : BasePaginatedListRequest
{
}