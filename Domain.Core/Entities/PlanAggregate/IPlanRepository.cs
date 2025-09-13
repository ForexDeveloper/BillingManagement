using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Core.Entities.PlanAggregate
{
    public interface IPlanRepository
    {
        Task AddAsync(Plan cmd);
        void Update(Plan cmd);
        Task<Plan> GetByIdAsync(int id);
        Task<bool> CheckPlanAsync(int id);
        Task<bool> CheckPlansExistAsync(List<int> ids);
        Task<bool> CheckPlansBelongToTenantAsync(List<int> ids, int tenantId);
        Task<bool> HasWalletContractPlanByPlanId(int id);
        Task<bool> CheckPlanMaxWalletAsync(int walletConfigurationId, decimal maxWallet);
        Task<bool> CheckPlanMaxTotalCreditAsync(int walletConfigurationId, decimal maxTotalCredit);
        Task<List<PlanDetailInstallment>> GetPlanDetailInstallmentsAsync(List<int> planDetailInstallmentIds);
    }
}
