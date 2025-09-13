using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.OrganizationAggregate.Exceptions
{
    public class OrganizationNotFoundException : NotFoundException
    {
        public OrganizationNotFoundException(string message) : base($"{message}")
        {

        }

    }
}