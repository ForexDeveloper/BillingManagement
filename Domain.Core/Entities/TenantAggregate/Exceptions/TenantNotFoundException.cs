using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.TenantAggregate.Exceptions
{
    public class TenantNotFoundException : NotFoundException
    {
        public TenantNotFoundException(string message) : base($"{message}")
        {
        }
    }
}