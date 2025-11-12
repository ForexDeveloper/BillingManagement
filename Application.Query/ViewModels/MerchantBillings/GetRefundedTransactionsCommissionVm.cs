using System.Collections.Generic;

namespace Application.Query.ViewModels.MerchantBillings;

public sealed record GetRefundedTransactionsCommissionVm
{
    public long Id { get; set; }

    public int MerchantId { get; set; }

    public decimal Amount { get; set; }

    public IEnumerable<GetMerchantBillingContractVm> Contracts { get; set; }
}