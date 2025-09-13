using Domain.Core.Enums;

namespace Application.Query.QueryModels
{
    public class GetActiveWalletsListQueryModel
    {
        public WalletType WalletType { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public decimal Balance { get; set; }

    }
}
