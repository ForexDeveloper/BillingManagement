using Shared.EventBus.Contracts;

namespace Shared.EventBus.Events;

public class CmMerchantAddedOrUpdatedEvent : IEventSign
{
    public int MerchantId { get; set; }
    public int TenantId { get; set; }
    public string Title { get; set; }
    public byte IdentityType { get; set; }
    public byte SaleType { get; set; }
    public List<int> Categories { get; set; }
    public int BranchId { get; set; }
    public long TerminalId { get; set; }
    public byte Status { get; set; }

}
