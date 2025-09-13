using Domain.Core.Enums;

namespace Application.Query.ViewModels.Plans;

public class PlanFinancialDetailsVm
{
    public int PlanId { get; set; }
    public string PlanName { get; set; }
    public OperationalFeeType OperationalFeeType { get; set; }
    public decimal InstallmentAmount { get; set; }
    public decimal InterestAmount { get; set; }
    public decimal InterestPercent { get; set; }
    public decimal TotalRepayableAmount { get; set; }
    public decimal OperationalFeeAmount { get; set; }
    public int NumberOfInstallments { get; set; }
    public decimal InitialCreditAmount { get; set; }
    public decimal CreditAmount { get; set; }

    public bool IsMaxTotalCreditExceeded { get; set; }
}

