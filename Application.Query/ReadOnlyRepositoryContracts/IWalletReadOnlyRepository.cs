using System.Threading.Tasks;

namespace Application.Query.ReadOnlyRepositoryContracts;

public interface IWalletReadOnlyRepository
{
    Task<int?> GetWalletContractIdByWalletIdAsync(int tenantId, int walletId);
}
