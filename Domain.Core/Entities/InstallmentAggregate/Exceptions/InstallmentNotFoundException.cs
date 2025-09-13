using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.InstallmentAggregate.Exceptions;

public class InstallmentNotFoundException : NotFoundException
{
    public InstallmentNotFoundException(string message) : base($"{message}")
    {
    }
}