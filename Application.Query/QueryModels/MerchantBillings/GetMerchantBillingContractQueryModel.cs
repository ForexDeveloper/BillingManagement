using System;
using Domain.Core.Enums;
using System.Collections.Generic;
using Domain.Core.Entities.Shared;

namespace Application.Query.QueryModels.MerchantBillings;

public sealed record GetMerchantBillingContractQueryModel
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

    public List<TieredCommission> TieredCommissions { get; set; }
}