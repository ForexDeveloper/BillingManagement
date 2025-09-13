using Domain.Core.Entities.BankAccountAggregate;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.Repositories;

public class BankAccountRepository : IBankAccountRepository
{
    private readonly ApplicationDbContext _context;

    public BankAccountRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(BankAccount bankAccount)
    {
        await _context.BankAccounts.AddAsync(bankAccount);
    }

    public async Task<bool> BusinessIdentityHasIban(int businessIdentityId, string ibanId)
    {
        var result =
            await _context.BankAccounts
                .AnyAsync(ba => ba.BusinessIdentityId == businessIdentityId && ba.Iban == ibanId);

        return result;
    }

    public async Task<BankAccount> GetByIdAsync(int id)
    {
        return await _context.BankAccounts.FirstOrDefaultAsync(ba => ba.Id == id);
    }

    public async Task<BankAccount> GetByIdAsync(int id, int tenantId)
    {
        return await _context.BankAccounts.FirstOrDefaultAsync(ba => ba.Id == id && ba.TenantId == tenantId);
    }
}