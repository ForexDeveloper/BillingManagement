using Application.Query.Queries.IpgSettings;
using Application.Query.Queries.Tenants;
using Application.Query.QueryModels.IpgSettings;
using Application.Query.QueryModels.Tenants;
using Domain.Core.Entities.TenantAggregate;
using System.Threading.Tasks;

namespace Application.Query.ReadOnlyRepositoryContracts
{
    public interface ITenantReadOnlyRepository
    {
        Task<Tenant> GetAsync(int id);

        Task<TenantIpgSetting> TenantIpSettingGetAsync(int id, int? tenantId = null);
        Task<GetTenantIpgSettingQueryModel> TenantIpSettingGetAllAsync(GetTenantIpgSettingsQuery request, int platformTenantId);
        Task<GetTenantsQueryModel> GetTenantsAsync(GetTenantsQuery request);
    }
}
