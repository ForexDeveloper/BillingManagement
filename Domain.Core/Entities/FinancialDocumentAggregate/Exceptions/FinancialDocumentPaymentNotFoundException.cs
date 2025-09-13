using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.FinancialDocumentAggregate.Exceptions;

public class FinancialDocumentPaymentNotFoundException : NotFoundException
{
    public FinancialDocumentPaymentNotFoundException(string message) : base($"{message}")
    {
    }
}