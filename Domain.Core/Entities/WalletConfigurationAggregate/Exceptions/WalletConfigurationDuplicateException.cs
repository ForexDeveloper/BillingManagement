using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.AggregateRoots.WalletConfigurationAggregate.Exceptions
{
    public class WalletConfigurationDuplicateException : DuplicateException
    {
        public WalletConfigurationDuplicateException(string iban) : base($"this {iban} exist on database")
        {

        }
    }
}