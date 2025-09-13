using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.TenantMerchantContractAggregate.Exceptions
{
    public class TenantMerchantContractStatusException : UnprocessableActionException
    {
        public TenantMerchantContractStatusException(string message) : base($"{message}")
        {
        }
    }
}