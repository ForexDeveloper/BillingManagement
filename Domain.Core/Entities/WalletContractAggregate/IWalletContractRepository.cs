using System.Threading.Tasks;

namespace Domain.Core.Entities.WalletContractAggregate;

public interface IWalletContractRepository
{
    Task AddAsync(WalletContract contract);
    void Update(WalletContract contract);
    Task<WalletContract> GetAsync(int id);
}