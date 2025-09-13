using Domain.Core.Enums;
using Domain.Core.Helper;

namespace Application.Query.ViewModels.Wallets
{
    public class GetActiveWalletViewModel
    {
        public WalletType Type { get; set; }
        public string TypeTitle => Type.GetEnumDescription();
        public int Id { get; set; }
        public string Title { get; set; }
        public decimal Balance { get; set; }
    }
}
