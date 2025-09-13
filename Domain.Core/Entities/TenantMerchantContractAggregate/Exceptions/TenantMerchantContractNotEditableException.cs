using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.TenantMerchantContractAggregate.Exceptions
{
    public class TenantMerchantContractNotEditableException : UnprocessableActionException
    {
        public TenantMerchantContractNotEditableException(string message) : base($"{message}")
        {
        }
    }
}