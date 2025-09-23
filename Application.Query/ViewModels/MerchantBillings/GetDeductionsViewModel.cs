namespace Application.Query.ViewModels.MerchantBillings;

public sealed record GetDeductionsViewModel
{
    public long Id { get; set; }

    public decimal Amount { get; set; }
}