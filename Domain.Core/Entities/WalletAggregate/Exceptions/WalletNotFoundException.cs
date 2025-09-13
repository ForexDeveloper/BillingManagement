using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.WalletAggregate.Exceptions
{
    public class WalletNotFoundException : NotFoundException
    {
        public WalletNotFoundException(string message) : base($"{message}")
        {
        }
    }
}