using Shared.EventBus.Contracts;

namespace Shared.EventBus.Events;

public class CgmProcessAddedOrUpdatedEvent : IEventSign
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; }
    public int State { get; set; }
    public List<int> PlanIds { get; set; } = [];
}
