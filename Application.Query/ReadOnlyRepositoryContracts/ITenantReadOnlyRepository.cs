using Application.Query.Queries;
using Application.Query.QueryModels;
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
