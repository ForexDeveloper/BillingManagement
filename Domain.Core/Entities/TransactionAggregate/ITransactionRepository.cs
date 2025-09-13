using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Core.Entities.TransactionAggregate;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction);
    Task AddRangeAsync(List<Transaction> transactions);
    void UpdateRange(List<Transaction> transactions);
    Task<Transaction> GetByIdAsync(long id);
    Task<List<Transaction>> GetAllAsync();

}
