using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.MerchantAggregate.Exceptions
{
    public class InvalidMerchantTypeException : ValidationException
    {
        public InvalidMerchantTypeException(string message) : base($"{message}")
        {

        }
    }
}