using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.BillingAggregate.Exceptions;

public class BillingPaymentException : UnprocessableActionException
{
    public BillingPaymentException(string message) : base($"{message}")
    {
    }
}