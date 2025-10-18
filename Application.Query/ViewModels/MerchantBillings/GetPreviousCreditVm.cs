using System;

namespace Application.Query.ViewModels.MerchantBillings;

public sealed record GetPreviousCreditVm
{
    public long Id { get; set; }

    public decimal Amount { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }
}