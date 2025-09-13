using Shared.EventBus.Contracts;

namespace Shared.EventBus.Events
{
    public class CmMerchantBranchAddedOrUpdatedEvent : IEventSign
    {
        public int BranchId { get; set; }
        public int MerchantId { get; set; }
        public string Title { get; set; }
        public long TerminalId { get; set; }
    }
}
