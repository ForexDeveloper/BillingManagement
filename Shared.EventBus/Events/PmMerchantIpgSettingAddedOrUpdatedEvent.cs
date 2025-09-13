using Shared.EventBus.Contracts;

namespace Shared.EventBus.Events;

public class PmMerchantIpgSettingAddedOrUpdatedEvent : IEventSign
{
    public int Id { get; set; }
    public int MerchantId { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public byte IpgType { get; set; }
}