using System;
using Domain.Core.Enums;
using System.Collections.Generic;

namespace Domain.Core.Entities.TenantMerchantContractAggregate.Dtos;

public sealed record ContractGroup
{
    public int TenantId { get; set; }

    public int MerchantId { get; set; }

    public bool HasEndorsement { get; set; }

    public required DateTime EndorsementDate { get; set; }

    public int BillingPeriod { get; set; }

    public TimeInterval BillingPeriodType { get; set; }

    public DateTime? BillingDailyOriginDate { get; set; }

    public IEnumerable<int> ContractIds { get; set; }
}