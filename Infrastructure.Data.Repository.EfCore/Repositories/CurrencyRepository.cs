using Domain.Core.Entities.CurrencyTypeAggregate;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.Repositories
{
    public class CurrencyRepository : ICurrencyRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public CurrencyRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public async Task<bool> CheckCurrency(int id)
        =>await _applicationDbContext.CurrencyTypes.AnyAsync(c=>c.Id== id);
    }
}
