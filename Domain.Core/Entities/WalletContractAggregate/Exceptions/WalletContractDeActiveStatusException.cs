using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.WalletContractAggregate.Exceptions
{
    public class WalletContractDeActiveStatusException : UnprocessableActionException
    {
        public WalletContractDeActiveStatusException(string message) : base($"{message}")
        {
        }
    }
}