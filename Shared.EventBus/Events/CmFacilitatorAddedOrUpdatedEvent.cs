using Shared.EventBus.Contracts;

namespace Shared.EventBus.Events;

public class CmFacilitatorAddedOrUpdatedEvent : IEventSign
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; }
    public byte Type { get; set; }
    public bool IsTenant { get; set; }

}