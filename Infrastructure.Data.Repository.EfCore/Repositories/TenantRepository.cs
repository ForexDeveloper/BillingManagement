using Domain.Core.Entities.TenantAggregate;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;

namespace Infrastructure.Data.Repository.EfCore.Repositories
{
    public class TenantRepository : ITenantRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public TenantRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task AddAsync(Tenant tenant)
        {
            await _applicationDbContext.Tenants.AddAsync(tenant);
        }

        public async Task<bool> CheckTenant(int id)
        {
            return await _applicationDbContext.Tenants.AnyAsync(p => p.Id == id);
        }

        public async Task TenantIPgSettingAddAsync(TenantIpgSetting ipgPaymentSettings)
        {
            await _applicationDbContext.TenantIpgSettings.AddAsync(ipgPaymentSettings);
        }

        public void TenantIPgSettingUpdate(TenantIpgSetting ipgPaymentSettings)
        {
            _applicationDbContext.TenantIpgSettings.Update(ipgPaymentSettings);
        }

        public async Task<TenantIpgSetting> TenantIPgSettingGetAsync(int id)
        {
            return await _applicationDbContext.TenantIpgSettings.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<TenantIpgSetting> TenantIPgSettingGetByTenantIdAsync(int tenantId)
        {
            return await _applicationDbContext.TenantIpgSettings.FirstOrDefaultAsync(p => p.TenantId == tenantId);
        }

        public async Task<Tenant> GetAsync(int id)
        {
            return await _applicationDbContext.Tenants.FirstOrDefaultAsync(p => p.Id == id);
        }

        public void Update(Tenant tenant)
        {
            _applicationDbContext.Tenants.Update(tenant);
        }
    }

}
