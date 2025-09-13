using Domain.Core.Entities.TenantPlatformContractAggregate;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.Repositories
{
    public class TenantPlatformContractRepository : ITenantPlatformContractRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public TenantPlatformContractRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task AddAsync(TenantPlatformContract contract)
        {
            await _applicationDbContext.TenantPlatformContracts.AddAsync(contract);
        }

        public void Update(TenantPlatformContract contract)
        {
            _applicationDbContext.TenantPlatformContracts.Update(contract);
        }

        public async Task<TenantPlatformContract> GetAsync(int id)
        {
            var contract = await _applicationDbContext.TenantPlatformContracts
               .Include(x => x.Providers).ThenInclude(x => x.Provider)
                .Include(x => x.Facilitators).ThenInclude(x => x.Facilitator)
               .Include(x => x.Tenant)
               .Include(x => x.TenantIpgSettings)
                .FirstOrDefaultAsync(x => x.Id == id);

            return contract;
        }

        public async Task<bool> IsDuplicatedContractNumberAsync(string contractNumber)
        {
            return await _applicationDbContext.TenantPlatformContracts.AnyAsync(x =>
                x.ContractNumber == contractNumber.Trim() && !x.IsDeleted);
        }

        public async Task<bool> IsExistsActiveContractAsync(int tenantId, int? contractId = null)
        {
            return await _applicationDbContext.TenantPlatformContracts.AnyAsync(x =>
                x.TenantId == tenantId && x.Status == true && !x.IsDeleted && x.EndDate >= System.DateTime.Now && (!contractId.HasValue || x.Id != contractId));
        }

        public async Task<TenantPlatformContract> GetByTenantIdAsync(int tenantId)
        {
            return await _applicationDbContext.TenantPlatformContracts
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Status == true && !x.IsDeleted && x.EndDate >= System.DateTime.Now);
        }

        public async Task<bool> HasEndorsement(int contractId, int tenantId)
        {
            return await _applicationDbContext.TenantPlatformContracts.AnyAsync(x => x.ParentId == contractId && x.TenantId == tenantId);
        }
    }
}
