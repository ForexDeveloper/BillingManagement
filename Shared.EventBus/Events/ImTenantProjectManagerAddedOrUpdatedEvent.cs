using Shared.EventBus.Contracts;

namespace Shared.EventBus.Events;

public class ImTenantProjectManagerAddedOrUpdatedEvent : IEventSign
{
    public int TenantId { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string FullName { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
}
