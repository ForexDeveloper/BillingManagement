using Shared.EventBus.Contracts;

namespace Shared.EventBus.Events;

public class FcmPlanAddedOrUpdatedEvent : IEventSign
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public decimal MaxWallet { get; set; }
    public List<PlanDetailEvent> PlanDetails { get; set; } = [];
}

public class PlanDetailEvent
{
    public int Id { get; set; }
    public List<int> NumberOfInstallments { get; set; } = [];
    public decimal? OperationFee { get; set; }
    public decimal? InterestPercent { get; set; }

}