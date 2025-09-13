using Application.Query.ViewModels.Attachments;
using Domain.Core.Enums;
using Domain.Core.Helper;
using System;
using System.Collections.Generic;

namespace Application.Query.ViewModels.Plans;

public class PlanViewModel
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
    public string BillingPeriodTypeTitle => BillingPeriodType?.GetEnumDescription();
    public DateTime? BillingPeriodStartDate { get; set; }
    public int? GracePeriod { get; set; }
    public TimeInterval? PaymentType { get; set; }
    public string PaymentTypeTitle => PaymentType?.GetEnumDescription();
    public TimeInterval? InstallmentBreakType { get; set; }
    public int? InstallmentBreak { get; set; }
    public string InstallmentBreakTypeTitle => InstallmentBreakType?.GetEnumDescription();
    public InstallmentPaymentMethodType? InstallmentPaymentMethod { get; set; }
    public string InstallmentPaymentMethodTitle => InstallmentPaymentMethod?.GetEnumDescription();
    public decimal? MaxDailyDeposit { get; set; }
    public decimal? MaxDailyTransactionCount { get; set; }
    public string BackgroundColor1 { get; set; }
    public string BackgroundColor2 { get; set; }
    public string TextColor { get; set; }
    public string Description { get; set; }
    public string Link { get; set; }
    public bool Editable { get; set; }
    public List<PlanClosedloopViewModel> PlanClosedloops { get; set; }
    public List<PlanDetailViewModel> PlanDetails { get; set; }
    public GetAttachmentVm PlanLogo { get; set; }
    public string TermsAndConditions { get; set; }
    public decimal ReservedCredit { get; set; }
    public decimal AssignedCredit { get; set; }
    public decimal RemainingCredit => MaxTotalCredit - ReservedCredit;

}
public class PlanClosedloopViewModel
{
    public int Id { get; set; }
    public int ClosedloopId { get; set; }
    public string Title { get; set; }

}
public class PlanDetailViewModel
{
    public int PlanId { get; set; }
    public int Id { get; set; }
    public List<int> NumberOfInstallments { get; set; }
    public decimal? OperationFee { get; set; }
    public OperationalFeeType? OperationalFeeType { get; set; }
    public string OperationalFeeTypeTitle => OperationalFeeType?.GetEnumDescription();
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