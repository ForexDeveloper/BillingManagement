using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.WalletConfigurationAggregate.Exceptions
{
    public class WalletConfigurationNotFoundException : NotFoundException
    {
        public WalletConfigurationNotFoundException(string message) : base($"{message}")
        {
        }
    }
}