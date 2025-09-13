using Domain.Core.Enums;

namespace Application.Query.QueryModels;

public record GetPlanInstallmentsQueryModel
{
    public int? InstallmentBreak { get; set; }

    public TimeInterval? InstallmentBreakType { get; set; }

    public decimal? InstallmentInterestPercent { get; set; }
    public decimal? OperationFee { get; set; }
    public int? BillingPeriod { get; set; }
}