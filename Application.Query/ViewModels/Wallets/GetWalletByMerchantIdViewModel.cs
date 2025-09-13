using Application.Service.Dtos.FileManagers;
using Domain.Core.Enums;
using System.Collections.Generic;

namespace Application.Query.ViewModels.Wallets
{
    public class GetWalletByMerchantIdViewModel
    {
        public int CustomerId { get; set; }
        public string CustomerFullName { get; set; }
        public string MerchantBranchTitle { get; set; }
        public List<GetWalletViewModel> Wallets { get; set; }
    }

    public class GetWalletViewModel
    {
        public int Id { get; set; }
        public int PlanId { get; set; }
        public WalletType WalletType { get; set; }
        public string WalletTypeTitle { get; set; }
        public WalletStatus WalletStatus { get; set; }
        public string WalletStatusTitle { get; set; }
        public bool IsDefault { get; set; }
        public string Title { get; set; }
        public decimal Balance { get; set; }
        public int Installment { get; set; }
        public DownloadDocumentsVm PlanLogo { get; set; }
        public decimal? PrepaymentPercent { get; set; }
        public decimal? PrepaymentMinAmount { get; set; }
        public decimal? PrepaymentMaxAmount { get; set; }
    }
}
