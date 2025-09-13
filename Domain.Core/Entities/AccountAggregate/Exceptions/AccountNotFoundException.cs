using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.AccountAggregate.Exceptions
{
    public class AccountNotFoundException : NotFoundException
    {
        public AccountNotFoundException(string message) : base($"{message}")
        {
        }
    }
}