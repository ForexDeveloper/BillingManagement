using Shared.EventBus.Contracts;

namespace Shared.EventBus.Events;

public class FcmProcessInstanceWalletIssuanceStatusUpdatedEvent : IEventSign
{
    public int UserCreditGrantingProcessId { get; set; }
    public bool IsSuccess { get; set; }
    public string Description { get; set; }
    public int? WalletId { get; set; }
}
