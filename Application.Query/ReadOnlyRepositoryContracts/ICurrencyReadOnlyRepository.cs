using Application.Query.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Query.ReadOnlyRepositoryContracts
{
    public interface ICurrencyReadOnlyRepository
    {
       Task<List<CurrencyQueryModel>> GetAllAsync();
    }
}
