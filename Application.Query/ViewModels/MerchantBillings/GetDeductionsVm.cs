namespace Application.Query.ViewModels.MerchantBillings;

public sealed record GetDeductionsVm
{
    public long Id { get; set; }

    public decimal Amount { get; set; }

    public string Description { get; set; }
}