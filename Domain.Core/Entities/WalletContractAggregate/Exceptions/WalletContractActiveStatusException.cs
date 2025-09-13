using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.WalletContractAggregate.Exceptions
{
    public class WalletContractActiveStatusException : UnprocessableActionException
    {
        public WalletContractActiveStatusException(string message) : base($"{message}")
        {
        }
    }
}