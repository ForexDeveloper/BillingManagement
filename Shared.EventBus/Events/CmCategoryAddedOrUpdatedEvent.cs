using Shared.EventBus.Contracts;

namespace Shared.EventBus.Events;

public class CmCategoryAddedOrUpdatedEvent : IEventSign
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public string Title { get; set; }
}
