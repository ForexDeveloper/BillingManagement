using Application.Query.Queries;
using Application.Query.QueryModels;
using Domain.Core.Entities.WalletContractAggregate;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Query.ReadOnlyRepositoryContracts
{
    public interface IWalletContractReadOnlyRepository
    {
        Task<WalletContractGetByIdQueryModel> GetByIdAsync(int id, int? tenantId);
        Task<WalletContractsQueryModel> GetRootParentListAsync(GetWalletContractsQuery request);
        Task<WalletContractCustomersQueryModel> GetCustomersListAsync(GetWalletContractsCustomerQuery request);
        Task<List<WalletContractRejectionQueryModel>> GetRejectionListAsync(int id);
        Task<bool> IsWalletContractBelongToTenantAsync(int walletContractId, int tenantId);
        Task<int?> GetTenantIpgSettingIdAsync(int id, int tenantId);
        Task<bool> HasCashWalletContractByIdAsync(int id);
        Task<List<WalletContractChildQueryModel>> GetEndorsementsByRootParentIdListAsync(int rootParentId, int? tenantId);
        Task<List<WalletContractChildQueryModel>> GetNotRejectedEndorsementsListAsync(int contractId, int? rootParentId, int tenantId);
        Task<WalletContractWithSameRootParentIdQueryModel> GetForCloneByContractIdAsync(int id, int tenantId);
        Task<bool> ExistsActiveOrDeactiveContractWithIdGreaterThan(int contractId, int? rootParentId, int tenantId);
        Task<WalletContract> GetActiveContractWithIdSmallerThan(int contractId, int? rootParentId, int tenantId);
        Task<bool> GetActiveContractByContractIdAsync(int id, int tenantId);
        Task<List<int>> GetWalletContractIdsHasCashWalletAsync(List<int> walletContractIds);
    }
}
