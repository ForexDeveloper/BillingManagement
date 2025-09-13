using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.TenantPlatformContractAggregate.Exceptions
{
    public class TenantPlatformContractStatusException : UnprocessableActionException
    {
        public TenantPlatformContractStatusException(string message) : base($"{message}")
        {
        }
    }
}