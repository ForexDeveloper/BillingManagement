using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Query.QueryModels;
using Application.Query.ReadOnlyRepositoryContracts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repository.EfCore.ReadonlyRepositories;

public class BankAccountReadOnlyRepository : IBankAccountReadOnlyRepository
{
    private readonly ReadonlyApplicationDbContext _dbContext;

    public BankAccountReadOnlyRepository(ReadonlyApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<GetCustomerBankAccountQueryModel>> GetCustomerBankAccountsByBusinessIdentityIdAsync(int customerId, int? tenantId=null)
    {
        var result = await _dbContext.BankAccounts
            .Where(ba => ba.BusinessIdentityId == customerId 
                         && (!tenantId.HasValue || tenantId.Value == ba.TenantId))
            .Select(ba => new GetCustomerBankAccountQueryModel(ba))
            .ToListAsync();
        return result;
    }
}