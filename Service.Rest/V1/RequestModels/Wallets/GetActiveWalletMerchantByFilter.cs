using Domain.Core.Enums;
using Shared.Utilities.Extensions;

namespace Service.Rest.V1.RequestModels.Wallets
{
    public class GetActiveWalletMerchantByFilter
    {
        public SaleType? SaleType { get; set; }
        public string CategoryList { get; set; }
        public string WalletList { get; set; }
        internal List<int> Categories => CategoryList.ToListInt();
        internal List<int> Wallets => WalletList.ToListInt();
        public string SearchValue { get; set; }

    }
}
