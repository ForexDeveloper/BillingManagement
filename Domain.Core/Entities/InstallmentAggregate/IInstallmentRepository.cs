using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Core.Entities.InstallmentAggregate;

public interface IInstallmentRepository
{
    Task AddAsync(Installment installment);
    Task AddRangeAsync(List<Installment> billings);
    void Update(Installment installment);
    void UpdateRange(List<Installment> installments);
    Task<List<int>> GetAccountIdsForCreateOrUpdateBilling();
    Task<List<int>> GetAccountIdsForCreateBilling();
    Task<Installment> GetById(long id);
    Task<List<Installment>> GetByIds(List<long> installmentIds);
    Task<List<Installment>> GetNotCompletePaidInstallmentsByAccountId(int accountId);
    Task<List<long>> GetParentInstallmentsWithoutBilling();
    Task<List<Installment>> GetInstallmentsByIds(List<long> installmentIds);
    Task<List<Installment>> GetAllAsync();
    Task<List<Installment>> GetInstallmentsByAccountId(int accountId);
    Task ExecuteUpdateAsync(List<long> installmentIds);
}
