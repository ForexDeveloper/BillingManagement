using Domain.Base;

namespace Domain.Core.Entities.WalletContractAggregate
{
    public class WalletContractRejectionReason : BaseEntity<int>
    {
        public int WalletContractId { get; private set; }
        public WalletContract WalletContract { get; private set; }
        public string? Reason { get; private set; }

        public WalletContractRejectionReason(string reason)
        {
            Reason = reason;
        }
    }
}
