using System.Collections.Generic;

namespace Application.Query.ViewModels.MerchantBillings;

public sealed record GetRefundedTransactionsCommissionViewModel
{
    public long Id { get; set; }

    public decimal Amount { get; set; }

    public IEnumerable<GetMerchantBillingContractViewModel> Contracts { get; set; }
}