using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.MerchantAggregate.Exceptions
{
    public class MerchantNotFoundException : NotFoundException
    {
        public MerchantNotFoundException(string message) : base($"{message}")
        {

        }
    }
}