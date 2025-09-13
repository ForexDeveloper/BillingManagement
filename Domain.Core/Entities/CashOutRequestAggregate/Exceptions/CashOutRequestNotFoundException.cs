using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.BankAccountAggregate.Exceptions;

public class CashOutRequestNotFoundException : NotFoundException
{
    public CashOutRequestNotFoundException(string message) : base($"{message}")
    {
    }
}