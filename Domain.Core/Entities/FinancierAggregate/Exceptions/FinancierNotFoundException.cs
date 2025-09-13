using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.FinancierAggregate.Exceptions
{
    public class FinancierNotFoundException : NotFoundException
    {
        public FinancierNotFoundException(string message) : base($"{message}")
        {
        }
    }
}