using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Core.Entities.MerchantAggregate
{
    public interface IMerchantRepository
    {
        Task AddAsync(Merchant merchant);

        void Update(Merchant merchant);

        Task<Merchant> GetAsync(int id, int tenantId);

        Task<Merchant> GetAsync(int id);

        Task<Merchant> GetByIdAsync(int id);

        Task<bool> CheckMerchantByTenantIdAsync(int tenantId, List<int> merchantIds);

        Task<MerchantBranch> GetBranchAsync(int id);

        Task<MerchantBranch> GetBranchByTerminalIdAsync(long terminalId);

        Task AddBranchAsync(MerchantBranch merchantBranch);

        void UpdateBranch(MerchantBranch merchantBranch);

        Task<int> GetMerchantIdByBranchId(int? branchId);
    }
}