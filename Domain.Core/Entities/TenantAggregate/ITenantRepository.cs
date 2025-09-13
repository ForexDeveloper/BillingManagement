using System.Threading.Tasks;

namespace Domain.Core.Entities.TenantAggregate
{
    public interface ITenantRepository
    {
        Task AddAsync(Tenant tenant);
        void Update(Tenant tenant);
        Task<Tenant> GetAsync(int id);
        Task<bool> CheckTenant(int id);

        Task TenantIPgSettingAddAsync(TenantIpgSetting ipgPaymentSettings);
        void TenantIPgSettingUpdate(TenantIpgSetting ipgPaymentSettings);
        Task<TenantIpgSetting> TenantIPgSettingGetAsync(int id);
        Task<TenantIpgSetting> TenantIPgSettingGetByTenantIdAsync(int tenantId);
    }
}