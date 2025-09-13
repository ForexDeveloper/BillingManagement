using System;
using Domain.Core.Enums;

namespace Domain.Core.Entities.TenantMerchantContractAggregate.ValueObjects;

public sealed record NoInstallmentContract
{
    public int Id { get; set; }

    public int TenantId { get; set; }

    public int MerchantId { get; set; }

    public int BillingPeriod { get; set; }

    public TimeInterval BillingPeriodType { get; set; }

    public DateTime? DailyBillingOriginDate { get; set; }

    public decimal? PeriodMaxCommissionAmount { get; set; }

    public decimal? PeriodMinCommissionAmount { get; set; }
}