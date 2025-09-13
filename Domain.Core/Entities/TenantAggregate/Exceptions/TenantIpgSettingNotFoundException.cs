using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.TenantAggregate.Exceptions
{
    public class TenantIpgSettingNotFoundException : NotFoundException
    {
        public TenantIpgSettingNotFoundException(string message) : base($"{message}")
        {
        }
    }
}