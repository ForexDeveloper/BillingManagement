using Application.Query.Base;

namespace Service.Rest.V1.RequestModels.Tenants;

public class GetAdminPurchasesListFilterModel : GetTenantPurchasesListFilterModel
{
    public int TenantId { get; set; }
}
public class GetTenantPurchasesListFilterModel : BasePaginatedListRequest
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<int> MerchantIds { get; set; } = [];
}