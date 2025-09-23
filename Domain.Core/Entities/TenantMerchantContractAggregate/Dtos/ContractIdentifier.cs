using System;
using Domain.Core.Enums;

namespace Domain.Core.Entities.TenantMerchantContractAggregate.Dtos;

/// <summary>
/// به هیچ وجه از رکورد به کلاس تبدیل نشود. جاب صورتسحاب منفجر می شود
/// </summary>
public sealed record ContractIdentifier
{
    public int TenantId { get; set; }

    public int MerchantId { get; set; }

    public int BillingPeriod { get; set; }

    public TimeInterval BillingPeriodType { get; set; }

    public DateTime? BillingDailyOriginDate { get; set; }

    public ContractIdentifier(int tenantId, int merchantId, int billingPeriod, TimeInterval billingPeriodType, DateTime? billingDailyOriginDate)
    {
        TenantId = tenantId;
        MerchantId = merchantId;
        BillingPeriod = billingPeriod;
        BillingPeriodType = billingPeriodType;
        BillingDailyOriginDate = billingDailyOriginDate;
    }

    public ContractIdentifier()
    {
        
    }
}
