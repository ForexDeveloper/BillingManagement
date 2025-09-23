namespace Application.Query.ViewModels.MerchantBillings;

public sealed record GetCurrentPeriodPurchaseTransactionsViewModel
{
     public long Id { get; set; }

    public int ContractId { get; set; }

    public decimal Amount { get; set; }
}