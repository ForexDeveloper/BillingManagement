using System.Threading.Tasks;

namespace Domain.Core.Entities.TenantPlatformContractAggregate
{
    public interface ITenantPlatformContractRepository
    {
        Task AddAsync(TenantPlatformContract contract);
        void Update(TenantPlatformContract contract);
        Task<TenantPlatformContract> GetAsync(int id);
        Task<bool> IsDuplicatedContractNumberAsync(string contractNumber);
        Task<bool> IsExistsActiveContractAsync(int tenantId, int? contractId = null);
        Task<TenantPlatformContract> GetByTenantIdAsync(int tenantId);
        Task<bool> HasEndorsement(int contractId, int tenantId);
    }
}