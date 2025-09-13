using Application.Query.Base;
using Domain.Core.Enums;

namespace Service.Rest.V1.RequestModels.Facilitators;

public class GetFacilitatorsModel : BasePaginatedListRequest
{
    public int? TenantId { get; set; }
    public IdentityTypeEnum? PersonType { get; set; }
}