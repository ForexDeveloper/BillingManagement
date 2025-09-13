using Application.Query.Base;
using Domain.Core.Enums;

namespace Service.Rest.V1.RequestModels.TenantPlatfromContracts;

public class GetTenantPlatformContractsModel : BasePaginatedListRequest
{
    public string ContractNumber { get; set; }
    public int? TenantId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public FeeCalculationType? FeeCalculationType { get; set; }
    public CommissionCalculationType? CommissionCalculationType { get; set; }
}