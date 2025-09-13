using Domain.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Core.Entities.AccountAggregate;

public interface IAccountRepository
{
    Task<Account> GetTenantAccountAsync(int businessIdentityId, AccountType accountType);
    Task AddAsync(Account account);
    Task AddRangeAsync(List<Account> accounts);
    void Update(Account account);
    void UpdateRange(List<Account> accounts);
    Task<Account> GetByIdAsync(int id);
    Task<Account> GetAsync(AccountType accountType, int businessIdentityId);
    Task<Account> GetAsync(int businessIdentityId);
    Task<List<Account>> GetListAsync(int businessIdentityId, int tenantId);
    Task<List<Account>> GetAllAsync();
    Task<List<Account>> GetByBusinessIdentityIds(List<int> businessIdentityIds, int tenantId);
    Task<Account> GetByIdWithPessimisticLockAsync(int id);
    Task<List<Account>> GetByIdsWithPessimisticLockAsync(List<int> ids);
}
