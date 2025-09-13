using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.GuarantorAggregate.Exceptions
{
    public class GuarantorNotFoundException : NotFoundException
    {
        public GuarantorNotFoundException(string message) : base($"{message}")
        {

        }
    }
}