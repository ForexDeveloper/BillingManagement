namespace Application.Query.ViewModels.MerchantBillings;

public sealed record GetPurchaseTransactionsViewModel
{
     public long Id { get; set; }

    public int ContractId { get; set; }

    public decimal Amount { get; set; }
}