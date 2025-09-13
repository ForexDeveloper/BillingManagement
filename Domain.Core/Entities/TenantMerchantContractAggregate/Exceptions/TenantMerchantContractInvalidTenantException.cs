using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.TenantMerchantContractAggregate.Exceptions
{
    public class TenantMerchantContractInvalidTenantException : ForbiddenException
    {
        public TenantMerchantContractInvalidTenantException(string message) : base($"{message}")
        {
        }
    }
}