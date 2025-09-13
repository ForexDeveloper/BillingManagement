using Domain.Core.Enums;

namespace Application.Query.ViewModels
{
    public class GetAllPlanQueryModel
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string TenantTitle { get; set; }
        public string Title { get; set; }
        public decimal MaxTotalCredit { get; set; }
        public decimal MaxWallet { get; set; }
        public string WalletConfigurationTitle { get; set; }
        public WalletType WalletTypeId { get; set; }
        public decimal? MaxDailyWithdrawal { get; set; }
        public decimal? MaxDailyDeposit { get; set; }
        public decimal? MaxDailyTransactionCount { get; set; }
        public string TermsAndConditions { get; set; }

    }
}