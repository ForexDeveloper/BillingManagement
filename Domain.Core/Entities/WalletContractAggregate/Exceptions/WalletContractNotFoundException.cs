using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.WalletContractAggregate.Exceptions
{
    public class WalletContractNotFoundException : NotFoundException
    {
        public WalletContractNotFoundException(string message) : base($"{message}")
        {
        }
    }
}