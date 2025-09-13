using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.WalletContractAggregate.Exceptions;

public class WalletContractAcceptanceStatusException : UnprocessableActionException
{
    public WalletContractAcceptanceStatusException(string message) : base($"{message}")
    {
    }
}