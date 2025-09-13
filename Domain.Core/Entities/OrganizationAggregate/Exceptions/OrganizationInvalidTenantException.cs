using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.OrganizationAggregate.Exceptions
{
    public class OrganizationInvalidTenantException : UnprocessableActionException
    {
        public OrganizationInvalidTenantException(string title) : base($"{title}")
        {

        }
    }
}
