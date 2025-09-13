using Domain.Core.Entities.WalletAggregate;
using Domain.Core.Enums;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.Repositories
{
    public class WalletRepository : IWalletRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public WalletRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<List<Wallet>> GetAsync(int businessIdentityId)
        {
            var wallets = await _applicationDbContext.Wallets
                .Where(x => x.BusinessIdentityId == businessIdentityId && x.Status == WalletStatus.Active).ToListAsync();

            return wallets;
        }

        public async Task<List<Wallet>> GetAsync(int businessIdentityId, int tenantId)
        {
            var wallets = await _applicationDbContext.Wallets
                .Include(x => x.Account)
                .Where(x => x.BusinessIdentityId == businessIdentityId && x.TenantId == tenantId && x is LoanWallet)
                .ToListAsync();

            return wallets;
        }

        public async Task<CashWallet> GetCashWalletAsync(int businessIdentityId)
        {
            var wallet = await _applicationDbContext.CashWallets
              .Where(x => x.BusinessIdentityId == businessIdentityId).FirstOrDefaultAsync();

            return wallet;
        }

        public async Task<Wallet> GetByIdAsync(int id)
        {
            var wallet = await _applicationDbContext.Wallets
                .Include(x => x.Plan).ThenInclude(x => x.PlanDetails)
                .Include(x => x.Plan).ThenInclude(x => x.PlanClosedloops)
                .ThenInclude(x => x.ClosedLoop).ThenInclude(x => x.ClosedloopMerchants)
                .FirstOrDefaultAsync(x => x.Id == id);
            return wallet;
        }

        public async Task<Wallet> GetByWalletIdAsync(int id)
        {
            var wallet = await _applicationDbContext.Wallets.FirstOrDefaultAsync(x => x.Id == id);
            return wallet;
        }

        public async Task AddAsync(Wallet wallet)
        {
            await _applicationDbContext.Wallets.AddAsync(wallet);
        }

        public void Update(Wallet wallet)
        {
            _applicationDbContext.Wallets.Update(wallet);
        }

        public async Task<LoanWallet> GetLoanWalletByUserCreditGrantingProcessIdAsync(int userCreditGrantingProcessId)
        {
            return await _applicationDbContext.LoanWallets
                .FirstOrDefaultAsync(x => x.UserCreditGrantingProcessId == userCreditGrantingProcessId);
        }

        public async Task<bool> HasLoanWalletAsync(int customerId, int tenantId)
        {
            return await _applicationDbContext.LoanWallets
                .AnyAsync(x => x.BusinessIdentityId == customerId && x.TenantId == tenantId);
        }

        public async Task<bool> HasCashWalletAsync(int businessIdentityId, int tenantId)
        {
            return await _applicationDbContext.CashWallets
                .AnyAsync(x => x.BusinessIdentityId == businessIdentityId && x.TenantId == tenantId);
        }

        public async Task<List<int>> GetCustomerIdsHasLoanWalletAsync(List<int> customerIds, int tenantId)
        {
            return await _applicationDbContext.LoanWallets
                .Where(x => customerIds.Contains(x.BusinessIdentityId) && x.TenantId == tenantId)
                .Select(x => x.BusinessIdentityId).ToListAsync();
        }

        public async Task<Dictionary<int, int>> GetWalletIdsAsync(List<int> accountIds, int? tenantId = null)
        {
            return await _applicationDbContext.Wallets
                .Where(x => accountIds.Contains(x.AccountId) && (!tenantId.HasValue || tenantId.Value == x.TenantId))
                .Select(x => new { AccountId = x.AccountId, WalletId = x.Id, PlanId = x.PlanId })
                .ToDictionaryAsync(
                    x => x.AccountId,
                    x => x.PlanId
                );
        }

        public async Task<Dictionary<int, decimal?>> GetPenaltyPercentByAccountIdsAsync(List<int> accountIds)
        {
            var result = await (
                from w in _applicationDbContext.Wallets
                join lw in _applicationDbContext.LoanWallets on w.Id equals lw.Id
                join p in _applicationDbContext.Plans on w.PlanId equals p.Id
                join pd in _applicationDbContext.PlanDetails on p.Id equals pd.PlanId
                join pdi in _applicationDbContext.PlanDetailInstallments on pd.Id equals pdi.PlanDetailId
                where pdi.NumberOfInstallment == lw.NumberOfInstallment
                    && accountIds.Contains(w.AccountId)
                select new
                {
                    w.AccountId,
                    pd.PenaltyPercent
                }
            ).ToDictionaryAsync(x => x.AccountId, x => x.PenaltyPercent);

            return result;
        }

        public async Task<bool> CheckWalletInitialAmountAsync(int planId, decimal maxWallet)
        => await _applicationDbContext.LoanWallets.Where(p => p.PlanId == planId &&
        p.InitialAmount > maxWallet).AnyAsync();

        public async Task<bool> CheckWalletMaxTotalCreditAsync(int planId, decimal maxTotalCredit)
        {
            var result = await _applicationDbContext.LoanWallets.Where(p => p.PlanId == planId)
            .GroupBy(p => p.WalletContract.OrganizationId)
            .Where(c => c.Sum(s => s.InitialAmount) > maxTotalCredit).AnyAsync();

            return result;
        }


    }

}
