using Domain.Core.Entities.OrganizationAggregate;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;

namespace Infrastructure.Data.Repository.EfCore.Repositories
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public OrganizationRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task AddAsync(Organization organization)
        {
            await _applicationDbContext.Organizations.AddAsync(organization);
        }

        public void Update(Organization organization)
        {
            _applicationDbContext.Organizations.Update(organization);
        }

        public async Task<Organization> GetAsync(int id)
        {
            return await _applicationDbContext.Organizations.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Organization> GetByTenantIdAsync(int tenantId)
        {
            return await _applicationDbContext.Organizations.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.ParentId == null);
        }
    }

}
