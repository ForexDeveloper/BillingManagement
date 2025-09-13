public class WalletContractPlanVm
{
    public int Id { get; set; }
    public int PlanId { get; set; }
    public string PlanTitle { get; set; }

    public WalletContractPlanVm(int id, int planId, string planTitle)
    {
        Id = id;
        PlanId = planId;
        PlanTitle = planTitle;
    }
}