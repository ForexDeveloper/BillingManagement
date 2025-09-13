using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.WalletContractsAggregate.Exceptions;

public class FinancialDocumentAlreadyExistsException : UnprocessableActionException
{
    public FinancialDocumentAlreadyExistsException(string message) : base($"{message}")
    {
    }
}