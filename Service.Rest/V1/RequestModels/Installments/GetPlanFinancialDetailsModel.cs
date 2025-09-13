using Domain.Core.Enums;

namespace Service.Rest.V1.RequestModels.Transactions;

public class PlanFinancialDetailsModel
{
    public OperationalFeeType OperationalFeeType { get; set; }
    public decimal CreditAmount { get; set; }
    public int PlanDetailInstallmentId { get; set; }
}
