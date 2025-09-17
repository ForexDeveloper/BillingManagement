using System.Threading.Tasks;

namespace Application.Query.ReadOnlyRepositoryContracts
{
    public interface IWalletContractReadOnlyRepository
    {
        Task<int?> GetTenantIpgSettingIdAsync(int id, int tenantId);
    }
}
