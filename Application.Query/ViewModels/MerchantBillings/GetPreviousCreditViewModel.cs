using System;

namespace Application.Query.ViewModels.MerchantBillings;

public sealed record GetPreviousCreditViewModel
{
    public long Id { get; set; }

    public decimal Amount { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }
}