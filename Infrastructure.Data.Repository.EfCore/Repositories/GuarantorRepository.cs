using Domain.Core.Entities.GuarantorAggregate;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.Repositories
{
    public class GuarantorRepository : IGuarantorRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public GuarantorRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task AddAsync(Guarantor guarantor)
        {
            await _applicationDbContext.Guarantors.AddAsync(guarantor);
        }

        public void Update(Guarantor guarantor)
        {
            _applicationDbContext.Guarantors.Update(guarantor);
        }

        public async Task<Guarantor> GetAsync(int id)
        {
            return await _applicationDbContext.Guarantors.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> IsGuarantorBelongToTenantAsync(int tenantId, int guarantorId)
        {
            return await _applicationDbContext.Guarantors.AnyAsync(p => p.Id == guarantorId && p.TenantId == tenantId);
        }

        public async Task<Guarantor> GetByTenantIdAsync(int tenantId)
        {
            return await _applicationDbContext.Guarantors.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.IsTenant);
        }
    }

}
