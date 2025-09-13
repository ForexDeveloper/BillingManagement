using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Core.Entities.WalletAggregate;

public interface IWalletRepository
{
    Task<Wallet> GetByIdAsync(int id);
    Task<Wallet> GetByWalletIdAsync(int id);
    Task<List<Wallet>> GetAsync(int businessIdentityId);
    Task<List<Wallet>> GetAsync(int businessIdentityId, int tenantId);
    Task AddAsync(Wallet wallet);
    void Update(Wallet wallet);
    Task<LoanWallet> GetLoanWalletByUserCreditGrantingProcessIdAsync(int userCreditGrantingProcessId);
    Task<bool> HasLoanWalletAsync(int businessIdentityId, int tenantId);
    Task<bool> HasCashWalletAsync(int businessIdentityId, int tenantId);
    Task<List<int>> GetCustomerIdsHasLoanWalletAsync(List<int> customerIds, int tenantId);
    Task<Dictionary<int, int>> GetWalletIdsAsync(List<int> accountIds, int? tenantId = null);
    Task<Dictionary<int, decimal?>> GetPenaltyPercentByAccountIdsAsync(List<int> accountIds);
    Task<CashWallet> GetCashWalletAsync(int businessIdentityId);
    Task<bool> CheckWalletInitialAmountAsync(int planId, decimal maxWallet);
    Task<bool> CheckWalletMaxTotalCreditAsync(int planId, decimal maxTotalCredit);
}
