using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.TenantPlatformContractAggregate.Exceptions
{
    public class TenantPlatformContractNotEditableException : UnprocessableActionException
    {
        public TenantPlatformContractNotEditableException(string message) : base($"{message}")
        {
        }
    }
}