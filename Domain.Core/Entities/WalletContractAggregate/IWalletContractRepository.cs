using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Core.Entities.WalletContractAggregate;

public interface IWalletContractRepository
{
    Task AddAsync(WalletContract contract);
    Task AddWalletContractBusinessIdentityAsync(WalletContractBusinessIdentity walletContractBusinessIdentity);
    void Update(WalletContract contract);
    Task<WalletContract> GetAsync(int id, int? tenantId = null);
    Task<WalletContract> GetByGrantingProcessIdAsync(int grantingProcessId, int tenantId);
    Task<WalletContract> GetAsync(int id);
    Task<WalletContract> GetByGrantingProcessIdAsync(int grantingProcessId);
    Task<WalletContractBusinessIdentity> GetWalletContractBusinessIdentityAsync(int walletContractId, int businessIdentityId);
    Task<List<WalletContractBusinessIdentity>> GetCustomersWithoutWalletAsync(int walletContractId);
    Task<List<int>> GetContractsWithCustomerWithoutWalletAsync();
    Task<WalletContractPlan> GetWalletContractPlanForCashWallet(int tenantId);
    Task<bool> ExistsContractByorganizationIdAndPlanIdsAsync(int tenantId, int organizationId, List<int> planIds);
    Task<bool> HasEndorsement(int contractId, int tenantId);
}