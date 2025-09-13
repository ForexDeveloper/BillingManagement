using Application.Query.Base;
using Domain.Core.Enums;

namespace Application.Query.QueryModels;

public class WalletsQueryModel : BasePaginatedListQueryResult<GetWalletsListQueryModel>
{
}

public class GetWalletsListQueryModel
{
    public int TenantId { get; set; }
    public int PlanId { get; set; }
    public WalletType WalletType { get; set; }
    public int Id { get; set; }
    public string Title { get; set; }
    public decimal Balance { get; set; }
    public string OrganizationTitle { get; set; }
    public WalletStatus WalletStatus { get; set; }
    public bool IsDefault { get; set; }

}
