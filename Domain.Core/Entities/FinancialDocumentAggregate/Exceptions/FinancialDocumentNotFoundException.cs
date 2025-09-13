using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.FinancialDocumentAggregate.Exceptions;

public class FinancialDocumentNotFoundException : NotFoundException
{
    public FinancialDocumentNotFoundException(string message) : base($"{message}")
    {
    }
}