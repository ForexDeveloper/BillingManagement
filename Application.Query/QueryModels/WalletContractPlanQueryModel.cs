namespace Application.Query.QueryModels;

public class WalletContractPlanQueryModel
{
    public int Id { get; set; }
    public int PlanId { get; set; }
    public string PlanTitle { get; set; }

    public WalletContractPlanQueryModel(int id, int planId, string planTitle)
    {
        Id = id;
        PlanId = planId;
        PlanTitle = planTitle;
    }
}