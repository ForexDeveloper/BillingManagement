using Domain.Core.Entities.AccountAggregate;
using Domain.Core.Enums;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public AccountRepository(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public async Task<Account> GetTenantAccountAsync(int businessIdentityId, AccountType accountType)
    {
        return await _applicationDbContext.Accounts.SingleOrDefaultAsync(x => x.BusinessIdentityId == businessIdentityId
            && x.Type == accountType && x.Status == AccountStatus.Active);
    }

    public async Task AddAsync(Account account)
    {
        await _applicationDbContext.Accounts.AddAsync(account);
    }

    public async Task AddRangeAsync(List<Account> accounts)
    {
        await _applicationDbContext.Accounts.AddRangeAsync(accounts);
    }

    public async Task<Account> GetByIdAsync(int id)
    {
        return await _applicationDbContext.Accounts.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Account> GetAsync(AccountType accountType, int businessIdentityId)
    {
        return await _applicationDbContext.Accounts.FirstOrDefaultAsync(p => p.Type == accountType && p.BusinessIdentityId == businessIdentityId);
    }

    public void Update(Account account)
    {
        _applicationDbContext.Accounts.Update(account);
    }

    public void UpdateRange(List<Account> accounts)
    {
        _applicationDbContext.Accounts.UpdateRange(accounts);
    }

    public async Task<Account> GetAsync(int businessIdentityId)
    {
        return await _applicationDbContext.Accounts.FirstOrDefaultAsync(p => p.BusinessIdentityId == businessIdentityId);
    }

    public async Task<List<Account>> GetListAsync(int businessIdentityId, int tenantId)
    {
        return await _applicationDbContext.Accounts.Where(p => p.BusinessIdentityId == businessIdentityId && p.TenantId == tenantId).ToListAsync();
    }

    public async Task<List<Account>> GetAllAsync()
    {
        return await _applicationDbContext.Accounts.ToListAsync();
    }

    public async Task<List<Account>> GetByBusinessIdentityIds(List<int> businessIdentityIds, int tenantId)
    {
        return await _applicationDbContext.Accounts.Where(x => businessIdentityIds.Contains(x.BusinessIdentityId) && x.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<Account> GetByIdWithPessimisticLockAsync(int id)
    {
        return await _applicationDbContext.Accounts
             .FromSqlRaw("SELECT * FROM Fc.Account WITH (UPDLOCK, ROWLOCK) WHERE Id = {0}", id)
             .FirstOrDefaultAsync();
    }

    public async Task<List<Account>> GetByIdsWithPessimisticLockAsync(List<int> ids)
    {
        var joinedIds = string.Join(", ", ids);
        var sql = $"SELECT * FROM Fc.Account WITH (UPDLOCK, ROWLOCK) WHERE Id IN ({joinedIds})";

        return await _applicationDbContext.Accounts
            .FromSqlRaw(sql)
            .ToListAsync();
    }
}
