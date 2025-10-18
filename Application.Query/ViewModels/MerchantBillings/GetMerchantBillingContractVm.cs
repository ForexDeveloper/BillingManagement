using System;
using Domain.Core.Enums;
using System.Collections.Generic;

namespace Application.Query.ViewModels.MerchantBillings;

public sealed record GetMerchantBillingContractVm
{
    public int Id { get; set; }

    public bool Status { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Description { get; set; }

    public decimal? FixedAmountCommission { get; set; }

    public decimal? FixedPercentageCommission { get; set; }

    public decimal? PeriodMinCommissionAmount { get; set; }

    public decimal? PeriodMaxCommissionAmount { get; set; }

    public decimal? TransactionMinCommissionAmount { get; set; }

    public decimal? TransactionMaxCommissionAmount { get; set; }

    public string CommissionCalculationTypeTitle { get; set; }

    public CommissionCalculationType? CommissionCalculationType { get; set; }

    public IEnumerable<TieredCommissionVm> TieredCommissions { get; set; }
}

public sealed record TieredCommissionVm
{
    public bool Selected { get; set; }

    public decimal FromAmount { get; set; }

    public decimal? ToAmount { get; set; }

    public decimal Percentage { get; set; }

    public decimal? MinAmount { get; set; }

    public decimal? MaxAmount { get; set; }
}