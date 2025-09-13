using Application.Query.Base;
using Domain.Core.Enums;

namespace Service.Rest.V1.RequestModels.Financiers;

public class GetTenantFinanciersModel : BasePaginatedListRequest
{
    public IdentityTypeEnum? PersonType { get; set; }
}