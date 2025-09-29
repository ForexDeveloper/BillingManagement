using System;
using Domain.Core.Enums;
using System.Collections.Generic;

namespace Application.Query.ViewModels.MerchantBillings;

public sealed record GetMerchantBillingContractViewModel
{
    public int Id { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public decimal? FixedAmountCommission { get; set; }

    public decimal? FixedPercentageCommission { get; set; }

    public decimal? PeriodMinCommissionAmount { get; set; }

    public decimal? PeriodMaxCommissionAmount { get; set; }

    public decimal? TransactionMinCommissionAmount { get; set; }

    public decimal? TransactionMaxCommissionAmount { get; set; }

    public string CommissionCalculationTypeTitle { get; set; }

    public BmCommissionCalculationType? CommissionCalculationType { get; set; }

    public IEnumerable<TieredCommissionViewModel> TieredCommissions { get; set; }
}

public sealed record TieredCommissionViewModel
{
    public decimal FromAmount { get; set; }

    public decimal? ToAmount { get; set; }

    public decimal? MinAmount { get; set; }

    public decimal? MaxAmount { get; set; }

    public decimal Percentage { get; set; }
}