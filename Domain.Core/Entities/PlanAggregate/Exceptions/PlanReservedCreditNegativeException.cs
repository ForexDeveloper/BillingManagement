using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.PlanAggregate.Exceptions;

public class PlanReservedCreditNegativeException : UnprocessableActionException
{
    public PlanReservedCreditNegativeException(string message) : base($"{message}")
    {

    }
}