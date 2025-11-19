using System.Collections.Generic;
using Domain.Core.Entities.Shared;

namespace Application.Query.QueryModels.MerchantBillings;

public sealed record GetPurchaseTransactionsCommissionQueryModel
{
    public long BillingId { get; set; }

    public int MerchantId { get; set; }

    public string Message { get; set; }

    public int MainContractId { get; set; }

    public decimal FinalAmount { get; set; }

    public int TransactionsCount { get; set; }

    public decimal CalculatedAmount { get; set; }

    public decimal TransactionsAmount { get; set; }

    public decimal TieredTransactionsAmount { get; set; }

    public IEnumerable<TieredCalculatedLevel> TieredCalculatedLevels { get; set; }

    public IEnumerable<GetMerchantBillingContractQueryModel> Contracts { get; set; }
}