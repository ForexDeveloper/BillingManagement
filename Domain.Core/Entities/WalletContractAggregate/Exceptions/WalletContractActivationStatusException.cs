using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.WalletContractAggregate.Exceptions
{
    public class WalletContractActivationStatusException : UnprocessableActionException
    {
        public WalletContractActivationStatusException(string message) : base($"{message}")
        {
        }
    }
}