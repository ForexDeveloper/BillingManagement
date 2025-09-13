using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.Repositories
{
    public class CurrencyReadOnlyRepository : ICurrencyReadOnlyRepository
    {
        private readonly ReadonlyApplicationDbContext _readonlyApplicationDbContext;

        public CurrencyReadOnlyRepository(ReadonlyApplicationDbContext readonlyApplicationDbContext)
        {
            _readonlyApplicationDbContext = readonlyApplicationDbContext;
        }

        public async Task<List<CurrencyQueryModel>> GetAllAsync()
        => await _readonlyApplicationDbContext.CurrencyTypes.Select(c => new CurrencyQueryModel
        {
            Id = c.Id,
            Title = c.Title
        }).ToListAsync();
    }

}
