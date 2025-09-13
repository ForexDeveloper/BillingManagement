using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.MerchantAggregate.Exceptions
{
    public class MerchantBranchNotFoundException : NotFoundException
    {
        public MerchantBranchNotFoundException(string message) : base($"{message}")
        {

        }
    }
}