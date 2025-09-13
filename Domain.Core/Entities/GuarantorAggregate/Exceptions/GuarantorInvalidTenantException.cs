using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.GuarantorAggregate.Exceptions
{
    public class GuarantorInvalidTenantException : UnprocessableActionException
    {
        public GuarantorInvalidTenantException(string title) : base($"{title}")
        {
        }

    }
}
