using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.PlanAggregate.Exceptions;

public class PlanMaxTotalCreditExceededException : UnprocessableActionException
{
    public PlanMaxTotalCreditExceededException(string message) : base($"{message}")
    {

    }
}