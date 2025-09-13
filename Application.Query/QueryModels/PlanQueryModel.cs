using Domain.Core.Enums;
using System;
using System.Collections.Generic;

namespace Application.Query.QueryModels;

public class PlanQueryModel
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string TenantTitle { get; set; }
    public int WalletConfigurationId { get; set; }
    public string WalletConfigurationTitle { get; set; }
    public string Title { get; set; }
    public decimal? MaxDailyWithdrawal { get; set; }
    public decimal MaxWallet { get; set; }
    public decimal MaxTotalCredit { get; set; }
    public TimeInterval? BillingPeriodType { get; set; }
    public int? BillingPeriod { get; set; }
    public DateTime? BillingPeriodStartDate { get; set; }
    public int? GracePeriod { get; set; }
    public TimeInterval? PaymentType { get; set; }
    public TimeInterval? InstallmentBreakType { get; set; }
    public int? InstallmentBreak { get; set; }
    public InstallmentPaymentMethodType? InstallmentPaymentMethod { get; set; }
    public decimal? MaxDailyDeposit { get; set; }
    public decimal? MaxDailyTransactionCount { get; set; }
    public string BackgroundColor1 { get; set; }
    public string BackgroundColor2 { get; set; }
    public string TextColor { get; set; }
    public string Description { get; set; }
    public string Link { get; set; }
    public IEnumerable<PlanDetailQueryModel> PlanDetails { get; set; }
    public IEnumerable<PlanClosedloopQueryModel> PlanClosedloops { get; set; }
    public string TermsAndConditions { get; set; }
    public decimal ReservedCredit { get; set; }
    public decimal AssignedCredit { get; set; }
}
public class PlanDetailQueryModel
{
    public int PlanId { get; set; }
    public int Id { get; set; }
    public IEnumerable<int> NumberOfInstallments { get; set; }
    public decimal? OperationFee { get; set; }
    public OperationalFeeType? OperationalFeeType { get; set; }
    public decimal? PenaltyPercent { get; set; }
    public decimal? PenaltyMaxAmount { get; set; }
    public decimal? PenaltyMinAmount { get; set; }
    public decimal? InterestPercent { get; set; }
    public decimal? InterestMaxAmount { get; set; }
    public decimal? InterestMinAmount { get; set; }
    public decimal? WaiverPercent { get; set; }
    public decimal? WaiverMaxAmount { get; set; }
    public decimal? WaiverMinAmount { get; set; }
    public decimal? PrepaymentPercent { get; set; }
    public decimal? PrepaymentMinAmount { get; set; }
    public decimal? PrepaymentMaxAmount { get; set; }

}
public class PlanClosedloopQueryModel
{
    public int Id { get; set; }
    public int ClosedloopId { get; set; }
    public string Title { get; set; }

}