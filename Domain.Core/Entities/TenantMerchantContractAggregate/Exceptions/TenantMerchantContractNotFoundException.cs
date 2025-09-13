using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.TenantMerchantContractAggregate.Exceptions
{
    public class TenantMerchantContractNotFoundException : NotFoundException
    {
        public TenantMerchantContractNotFoundException(string message) : base($"{message}")
        {
        }
    }
}