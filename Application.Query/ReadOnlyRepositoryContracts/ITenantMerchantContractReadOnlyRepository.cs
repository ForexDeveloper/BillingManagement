using Application.Query.Queries;
using Application.Query.QueryModels;
using Domain.Core.Entities.TenantMerchantContractAggregate;
using System.Threading.Tasks;

namespace Application.Query.ReadOnlyRepositoryContracts
{
    public interface ITenantMerchantContractReadOnlyRepository
    {
        Task<TenantMerchantContract> GetByIdAsync(int id, int? tenantId);
        Task<TenantMerchantContractsQueryModel> GetListAsync(GetTenantMerchantContractsQuery request);
        Task<bool> GetActiveContractAsync(int tenantId, int merchantId);

    }
}
