using Shared.EventBus.Contracts;

namespace Shared.EventBus.Events;

public class CmOrganizationAddedOrUpdatedEvent : IEventSign
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int TenantId { get; set; }
    public int? ParentId { get; set; }
}