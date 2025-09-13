using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities
{
    public class TenantForbiddenException : ForbiddenException
    {
        public TenantForbiddenException() : base()
        {
        }

        public TenantForbiddenException(string message) : base($"{message}")
        {
        }
    }
}