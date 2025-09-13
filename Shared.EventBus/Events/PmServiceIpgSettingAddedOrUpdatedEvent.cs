using Shared.EventBus.Contracts;

namespace Shared.EventBus.Events;

public class PmServiceIpgSettingAddedOrUpdatedEvent : IEventSign
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int TenantId { get; set; }
    public bool IsActive { get; set; }
    public byte IpgType { get; set; }
}