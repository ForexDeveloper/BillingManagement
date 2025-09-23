using Domain.Core.Entities.Providers;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;

namespace Infrastructure.Data.Repository.EfCore.Repositories
{
    public class ProviderRepository : IProviderRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public ProviderRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task AddAsync(Provider provider)
        {
            await _applicationDbContext.Providers.AddAsync(provider);
        }

        public void Update(Provider provider)
        {
            _applicationDbContext.Providers.Update(provider);
        }

        public async Task<Provider> GetAsync(int id)
        {
            return await _applicationDbContext.Providers.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> IsProviderExistsAsync(List<int> providersId)
        {
            return await _applicationDbContext.Providers
                .AnyAsync(c => providersId.Contains(c.Id));
        }

        public async Task<List<Provider>> GetProvidersAsync(List<int> providerds)
        {
            var providers = await _applicationDbContext.Providers
               .Where(c => providerds.Contains(c.Id))
               .ToListAsync();

            return providers;
        }
    }

}
