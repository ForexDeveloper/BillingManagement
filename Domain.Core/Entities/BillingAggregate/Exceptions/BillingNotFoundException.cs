using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.BillingAggregate.Exceptions;

public class BillingNotFoundException : NotFoundException
{
    public BillingNotFoundException(string message) : base($"{message}")
    {
    }
}