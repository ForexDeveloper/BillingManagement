namespace Application.Query.ViewModels.MerchantBillings;

public sealed record GetAdditionsViewModel
{
    public long Id { get; set; }

    public decimal Amount { get; set; }
}