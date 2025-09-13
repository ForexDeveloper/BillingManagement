using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.BankAccountAggregate.Exceptions;

public class BankAccountNotFoundException : NotFoundException
{
    public BankAccountNotFoundException(string message) : base($"{message}")
    {
    }
}