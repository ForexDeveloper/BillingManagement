using System;
using Domain.Core.Enums;
using System.Collections.Generic;
using Domain.Core.Entities.Shared;

namespace Domain.Core.Entities.TenantMerchantContractAggregate.Dtos;

public sealed record ContractGroup
{
    public int TenantId { get; set; }

    public int MerchantId { get; set; }

    public int BillingPeriod { get; set; }

    public TimeInterval BillingPeriodType { get; set; }

    public DateTime? BillingDailyOriginDate { get; set; }

    public bool HasEndorsement { get; set; }

    public required bool Status { get; set; }

    public DateTime EndorsementDate { get; set; }

    public IEnumerable<int> ContractIds { get; set; }

    public decimal? PeriodMinCommissionAmount { get; set; }

    public decimal? PeriodMaxCommissionAmount { get; set; }

    public List<TieredCommission> TieredCommissions { get; set; }

    public CommissionCalculationType CommissionCalculationType { get; set; }
}