using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.PlanAggregate.Exceptions
{
    public class PlanNotFoundException : NotFoundException
    {
        public PlanNotFoundException(string message) : base($"{message}")
        {

        }
    }
}
