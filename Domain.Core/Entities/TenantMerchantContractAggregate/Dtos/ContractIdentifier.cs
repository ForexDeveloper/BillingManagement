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

    public CommissionCalculationType CommissionCalculationType { get; set; }

    public ContractIdentifier(int tenantId, int merchantId, int billingPeriod, TimeInterval billingPeriodType,
        CommissionCalculationType commissionCalculationType)
    {
        TenantId = tenantId;
        MerchantId = merchantId;
        BillingPeriod = billingPeriod;
        BillingPeriodType = billingPeriodType;
        CommissionCalculationType = commissionCalculationType;
    }

    public ContractIdentifier(int tenantId, int merchantId, int billingPeriod, TimeInterval billingPeriodType,
        DateTime? billingDailyOriginDate, CommissionCalculationType commissionCalculationType)
    {
        TenantId = tenantId;
        MerchantId = merchantId;
        BillingPeriod = billingPeriod;
        BillingPeriodType = billingPeriodType;
        BillingDailyOriginDate = billingDailyOriginDate;
        CommissionCalculationType = commissionCalculationType;
    }

    public ContractIdentifier()
    {

    }
}

/// <summary>
/// به هیچ وجه از رکورد به کلاس تبدیل نشود. جاب صورتسحاب منفجر می شود
/// </summary>
public sealed record TenantMerchantIdentifier(int TenantId, int MerchantId)
{
    public int TenantId { get; set; } = TenantId;

    public int MerchantId { get; set; } = MerchantId;
}