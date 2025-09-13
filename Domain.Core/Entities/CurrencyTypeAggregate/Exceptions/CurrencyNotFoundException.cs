using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.CurrencyTypeAggregate.Exceptions
{
    public class CurrencyNotFoundException: NotFoundException
    {
        public CurrencyNotFoundException(string msg) :base(msg)
        {
                
        }
    }
}
