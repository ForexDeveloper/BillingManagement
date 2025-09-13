using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.Providers.Exceptions
{
    public class ProviderNotFoundException : NotFoundException
    {
        public ProviderNotFoundException(string message) : base($"{message}")
        {
        }
    }
}