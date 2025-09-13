using Domain.Core.Enums;
using Domain.Core.Helper;

namespace Application.Query.ViewModels.WalletConfigurations
{
    public class WalletConfigurationsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public WalletType WalletTypeId { get; set; }
        public decimal MaxWallet { get; set; }
        public int TenantId { get; set; }
        public string TenantTitle { get; set; }
        public int? ProjectManagerId { get; set; }
        public string ProjectManagerFullName { get; set; }
        public string WalletTypeTitle => WalletTypeId.GetEnumDescription();
        public int PlanCount { get; set; }

    }

}