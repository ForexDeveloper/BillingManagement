using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.WalletContractAggregate.Exceptions
{
    public class WalletContractInvalidTenantException : UnprocessableActionException
    {
        public WalletContractInvalidTenantException(string message) : base($"{message}")
        {
        }
    }
}