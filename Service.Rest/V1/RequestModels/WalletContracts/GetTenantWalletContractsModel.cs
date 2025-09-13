using Application.Query.Base;
using Domain.Core.Enums;

namespace Service.Rest.V1.RequestModels.WalletContracts;

public class GetTenantWalletContractsModel : BasePaginatedListRequest
{
    public List<int> OrganizationIds { get; set; }
    public WalletContractStatus? Status { get; set; }
    public WalletContractEndDateType? EndDate { get; set; }
    public string Title { get; set; }
}