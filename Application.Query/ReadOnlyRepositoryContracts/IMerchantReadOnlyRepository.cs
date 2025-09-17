using Application.Query.Queries;
using Application.Query.QueryModels;
using Domain.Core.Entities.MerchantAggregate;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Query.ReadOnlyRepositoryContracts
{
    public interface IMerchantReadOnlyRepository
    {
        Task<List<MerchantListQueryModel>> GetListAsync(int tenatid);
        Task<MerchantBranch> GetBranchByMerchantIdAsync(int merchantId);
        Task<MerchantBranch> GetBranchByIdAsync(int branchId);
        Task<MerchantBranch> GetMerchantBranchByTerminalIdAsync(long terminalId);
        Task<List<MerchantBranchSummeryQueryModel>> GetMerchantBranchesByTenantIdAsync(int merchantId, int? tenantId);
    }
}
