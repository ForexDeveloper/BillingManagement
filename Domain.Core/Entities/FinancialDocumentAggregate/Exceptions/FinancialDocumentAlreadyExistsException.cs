using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.FinancialDocumentAggregate.Exceptions;

public class FinancialDocumentAlreadyExistsException : UnprocessableActionException
{
    public FinancialDocumentAlreadyExistsException(string message) : base($"{message}")
    {
    }
}