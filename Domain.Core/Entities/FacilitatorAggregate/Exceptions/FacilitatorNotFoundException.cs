using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.FacilitatorAggregate.Exceptions
{
    public class FacilitatorNotFoundException : NotFoundException
    {
        public FacilitatorNotFoundException(string message) : base($"{message}")
        {
        }
    }
}