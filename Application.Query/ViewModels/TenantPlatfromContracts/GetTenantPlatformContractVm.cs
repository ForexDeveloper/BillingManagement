using Application.Service.Dtos.Shared;
using Domain.Core.Enums;
using System;
using System.Collections.Generic;

namespace Application.Query.ViewModels.TenantPlatfromContracts;

public class GetTenantPlatformContractVm
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string TenantName { get; set; }
    public string ContractNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Description { get; set; }
    public FeeCalculationType FeeCalculationType { get; set; }
    public string FeeCalculationTypeTitle { get; set; }
    public CommissionCalculationType CommissionCalculationType { get; set; }
    public string CommissionCalculationTypeTitle { get; set; }
    public decimal? FixedAmount { get; set; }
    public List<TieredCommissionDto> TieredCommissions { get; set; } = [];
    public decimal? FixedAmountCommission { get; set; }
    public decimal? FixedPercentageCommission { get; set; }
    public List<CommissionReferenceType> CommissionReferenceTypes { get; set; }
    public decimal? TransactionMinCommissionAmount { get; set; }
    public decimal? TransactionMaxCommissionAmount { get; set; }
    public decimal? PeriodMinCommissionAmount { get; set; }
    public decimal? PeriodMaxCommissionAmount { get; set; }
    public TimeInterval BillingPeriodType { get; set; }
    public string BillingPeriodTypeTitle { get; set; }
    public int BillingPeriod { get; set; }
    public DateTime? DailyBillingOriginDate { get; set; }
    public int? GracePeriod { get; set; }
    public decimal? PenaltyPercent { get; set; }
    public List<TenantPlatformContractFacilitatorVm> Facilitators { get; set; } = [];
    public List<TenantPlatformContractProviderVm> Providers { get; set; } = [];
    public int TenantIpgSettingId { get; set; }
    public IpgSettingOwnerType IpgSettingOwnerType { get; set; }
    public string IpgSettingOwnerTypeTitle { get; set; }
    public string TenantIpgSettingName { get; set; }
    public bool IsEditable { get; set; }
    public bool Status { get; set; }
}