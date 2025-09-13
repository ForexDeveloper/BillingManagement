using Domain.Core.Enums;

namespace Service.Rest.V1.RequestModels.Plans;

public class GetPlanInstallmentsModel
{
    public int NumberOfInstallments { get; set; }
    public decimal Amount { get; set; }
    public OperationalFeeType OperationalFeeType { get; set; }
}