using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.FacilitatorAggregate.Exceptions
{
    public class FacilitatorInvalidTenantException : UnprocessableActionException
    {
        public FacilitatorInvalidTenantException(string message) : base($"{message}")
        {
        }

    }
}
