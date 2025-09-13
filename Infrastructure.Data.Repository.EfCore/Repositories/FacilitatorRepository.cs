using Domain.Core.Entities.FacilitatorAggregate;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.Repositories
{
    public class FacilitatorRepository : IFacilitatorRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public FacilitatorRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task AddAsync(Facilitator facilitator)
        {
            await _applicationDbContext.Facilitators.AddAsync(facilitator);
        }

        public void Update(Facilitator facilitator)
        {
            _applicationDbContext.Facilitators.Update(facilitator);
        }

        public async Task<Facilitator> GetAsync(int id)
        {
            return await _applicationDbContext.Facilitators.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> FacilitatorsBelongToTenantAsync(List<int> facilitatorsId, int tenantId)
        {
            var tenantFacilitatorsId = await _applicationDbContext.Facilitators
                .Where(c => c.TenantId == tenantId)
                .Select(c => c.Id)
                .ToListAsync();

            return facilitatorsId.All(id => tenantFacilitatorsId.Contains(id));
        }

        public async Task<Facilitator> GetByTenantIdAsync(int tenantId)
        {
            return await _applicationDbContext.Facilitators.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.IsTenant);
        }
    }
}
