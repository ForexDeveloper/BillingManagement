using Domain.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Core.Entities.WalletContractAggregate;

public interface IWalletContractRepository
{
    Task AddAsync(WalletContract contract);
    void Update(WalletContract contract);
    void UpdateRange(List<WalletContract> walletContracts);
    Task<WalletContract> GetAsync(int id);
    Task<List<WalletContract>> GetByRootParentIdAsync(int rootParentId, WalletContractStatus status);

}