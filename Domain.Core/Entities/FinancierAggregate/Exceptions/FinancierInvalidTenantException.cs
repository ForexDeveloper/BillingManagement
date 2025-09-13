using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.FinancierAggregate.Exceptions
{
    public class FinancierInvalidTenantException : UnprocessableActionException
    {
        public FinancierInvalidTenantException(string message) : base($"{message}")
        {
        }
    }
}
