using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Core.Entities.CurrencyTypeAggregate
{
    public interface ICurrencyRepository
    {
        Task<bool> CheckCurrency(int id);

    }
}
