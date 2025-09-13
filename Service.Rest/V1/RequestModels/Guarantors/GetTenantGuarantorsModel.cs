using Application.Query.Base;
using Domain.Core.Enums;

namespace Service.Rest.V1.RequestModels.Guarantors;

public class GetTenantGuarantorsModel : BasePaginatedListRequest
{
    public int? Id { get; set; }
    public IdentityTypeEnum? PersonType { get; set; }
}