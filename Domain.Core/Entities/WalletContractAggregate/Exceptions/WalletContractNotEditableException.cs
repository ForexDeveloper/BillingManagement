using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.TenantMerchantContractAggregate.Exceptions
{
    public class WalletContractNotEditableException : UnprocessableActionException
    {
        public WalletContractNotEditableException(string message) : base($"{message}")
        {
        }
    }
}