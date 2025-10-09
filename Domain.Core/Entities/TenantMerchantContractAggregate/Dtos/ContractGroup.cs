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

    public DateTime? DailyBillingOriginDate { get; set; }

    public required bool Status { get; set; }

    public DateTime CreatedDateTime { get; set; }

    public DateTime EndorsementDate { get; set; }

    public List<int> ContractIds { get; set; }

    public decimal? PeriodMinCommissionAmount { get; set; }

    public decimal? PeriodMaxCommissionAmount { get; set; }

    public List<TieredCommission> TieredCommissions { get; set; }

    public CommissionCalculationType CommissionCalculationType { get; set; }

    public required List<CommissionReferenceType> CommissionReferenceTypes { get; set; }

    public ContractIdentifier CreateIdentifier()
    {
        return new ContractIdentifier(TenantId,
            MerchantId,
            BillingPeriod,
            BillingPeriodType,
            DailyBillingOriginDate,
            CommissionCalculationType);
    }
}