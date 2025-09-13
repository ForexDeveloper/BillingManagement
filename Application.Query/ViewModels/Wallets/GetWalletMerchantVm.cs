using Domain.Core.Enums;

namespace Application.Query.ViewModels.Wallets
{
    public class GetWalletMerchantVm
    {
        public TempateType Type { get; set; }
        public int? MerchantId { get; set; }
    }
}
