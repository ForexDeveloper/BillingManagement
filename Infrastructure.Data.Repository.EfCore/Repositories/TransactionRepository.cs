using Domain.Core.Entities.TransactionAggregate;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public TransactionRepository(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public async Task AddAsync(Transaction transaction)
    {
        await _applicationDbContext.Transactions.AddAsync(transaction);
    }

    public async Task AddRangeAsync(List<Transaction> transactions)
    {
        await _applicationDbContext.Transactions.AddRangeAsync(transactions);
    }

    public void UpdateRange(List<Transaction> transactions)
    {
        _applicationDbContext.Transactions.UpdateRange(transactions);
    }

    public async Task<Transaction> GetByIdAsync(long id)
    {
        return await _applicationDbContext.Transactions.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Transaction>> GetAllAsync()
    {
        return await _applicationDbContext.Transactions.ToListAsync();
    }
}
