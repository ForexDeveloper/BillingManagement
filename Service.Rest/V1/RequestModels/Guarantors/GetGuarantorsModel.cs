using Application.Query.Base;
using Domain.Core.Enums;

namespace Service.Rest.V1.RequestModels.Guarantors;

public class GetGuarantorsModel : BasePaginatedListRequest
{
    public int? TenantId { get; set; }
    public IdentityTypeEnum? PersonType { get; set; }
}