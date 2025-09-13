using Application.Query.Base;
using Domain.Core.Enums;

namespace Service.Rest.V1.RequestModels.WalletContracts
{
    public class GetWalletContractsModel : BasePaginatedListRequest
    {
        public int? TenantId { get; set; }
        public List<int> OrganizationIds { get; set; }
        public WalletContractEndDateType? EndDate { get; set; }
        public WalletContractStatus? Status { get; set; }
    }
}
