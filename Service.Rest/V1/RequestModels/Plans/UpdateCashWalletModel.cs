namespace Service.Rest.V1.RequestModels.Plans;

public class UpdateCashWalletConfigurationPlanModel
{
    public decimal MaxWallet { get; set; }
    public decimal MaxDailyWithdrawal { get; set; }
    public decimal? MaxDailyDeposit { get; set; }
    public decimal? MaxDailyTransactionCount { get; set; }
    public string TermsAndConditions { get; set; }

}

public class IncreasePlanReservedCreditAmountModel
{
    public decimal RequestedCreditAmount { get; set; }
}

public class DecreasePlanReservedCreditAmountModel
{
    public decimal RequestedCreditAmount { get; set; }
}