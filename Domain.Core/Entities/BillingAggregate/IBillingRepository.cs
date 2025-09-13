using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Core.Entities.BillingAggregate;

public interface IBillingRepository
{
    Task<Billing> GetByIdAsync(long id);
    Task AddAsync(Billing billing);
    Task AddRangeAsync(List<Billing> billings);
    void Update(Billing billing);
    void UpdateRange(List<Billing> transactions);
    Task<List<long>> GetInstallmentIdsAsync(long billingId);
    Task<bool> HasNotCalculatedPenalty(long billingId);
    Task<Billing> GetCurrentBillingByAccountIdAsync(int accountId);
    Task<Billing> GetLastBillingByAccountIdAsync(int accountId);
    Task<Billing> GetPreviousNotCompletePaidBillingAsync(int accountId, long billingId);
    Task<bool> IsLastBilling(int accountId, long billingId);
    Task<List<Billing>> GetBillingsByAccountIdAsync(int accountId);
    Task<List<Billing>> GetAllAsync();
    Task<bool> IsExistPreviousNotCompletePaidBillingAsync(int accountId, long billingId);
    Task<Billing> GetBillingByInstallmentIdAsync(long installmentId, int tenantId);
}
