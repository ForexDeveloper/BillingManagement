using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.ClosedloopAggregate.Exceptions
{
    public class ClosedloopNotFoundException : NotFoundException
    {
        public ClosedloopNotFoundException(string message) : base($"{message}")
        {
        }
    }
}
