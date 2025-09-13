using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.TenantPlatformContractAggregate.Exceptions
{
    public class TenantPlatformContractNotFoundException : NotFoundException
    {
        public TenantPlatformContractNotFoundException(string message) : base($"{message}")
        {
        }
    }
}