using Application.Query.Base;

namespace Service.Rest.V1.RequestModels.Merchants
{
    public class GetMerchantBranchPurchasesModel : BasePaginatedListRequest
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<int> WalletIds { get; set; } = [];
    }
}
