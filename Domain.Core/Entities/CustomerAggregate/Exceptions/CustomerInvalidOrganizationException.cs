using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.CustomerAggregate.Exceptions
{
    public class CustomerInvalidOrganizationException : UnprocessableActionException
    {
        public CustomerInvalidOrganizationException(string message) : base($"{message}")
        {
        }
    }
}
