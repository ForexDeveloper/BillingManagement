using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.CustomerAggregate.Exceptions
{
    public class CustomerNotFoundException : NotFoundException
    {
        public CustomerNotFoundException(string message) : base($"{message}")
        {
        }
    }
}