using Application.Command.PlanCommands;
using Domain.Core.Enums;

namespace Service.Rest.V1.RequestModels.Plans
{
    public class UpdatePlanModel
    {
        public string Title { get; set; }
        public decimal MaxTotalCredit { get; set; }
        public decimal MaxWallet { get; set; }
        public decimal? MaxDailyWithdrawal { get; set; }
        public decimal? MaxDailyDeposit { get; set; }
        public decimal? MaxDailyTransactionCount { get; set; }
        public List<int> PlanClosedloops { get; set; }
        public int WalletConfigurationId { get; set; }
        public string WalletLogo { get; set; }
        public string BackgroundColor1 { get; set; }
        public string BackgroundColor2 { get; set; }
        public string TextColor { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public TimeInterval BillingPeriodType { get; set; }
        public int BillingPeriod { get; set; }
        public DateTime? BillingPeriodStartDate { get; set; }
        public int? GracePeriod { get; set; }
        public TimeInterval? PaymentType { get; set; }
        public TimeInterval? InstallmentBreakType { get; set; }
        public int? InstallmentBreak { get; set; }
        public InstallmentPaymentMethodType? InstallmentPaymentMethod { get; set; }
        public List<UpdatePlanDetailDto> PlanDetails { get; set; }
        public string TermsAndConditions { get; set; }

    }
}