using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.PlanAggregate.Exceptions
{
    public class PlanInvalidTenantException : UnprocessableActionException
    {
        public PlanInvalidTenantException(string title) : base($"{title}")
        {

        }
    }
}
