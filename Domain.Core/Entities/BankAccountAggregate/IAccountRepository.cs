using System.Threading.Tasks;

namespace Domain.Core.Entities.BankAccountAggregate;

public interface IBankAccountRepository
{
    Task AddAsync(BankAccount bankAccount);

    Task<bool> BusinessIdentityHasIban(int businessIdentityId, string ibanId);

    Task<BankAccount> GetByIdAsync(int id);

    Task<BankAccount> GetByIdAsync(int id, int tenantId);
}
