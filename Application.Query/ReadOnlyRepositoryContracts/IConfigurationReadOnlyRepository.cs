using Application.Query.Queries;
using Application.Query.QueryModels;
using Application.Query.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Query.ReadOnlyRepositoryContracts
{
    public interface IWalletConfigurationReadOnlyRepository
    {
       Task<WalletConfigurationQueryModel> GetAsync(int id, int? tenantId = null);
        Task<GetWalletConfigurationQueryModel> GetListAsync(GetAllWalletConfigurationQuery request);
        Task<List<GetWalletConfigurationWithOutCashWalletQueryModel>> GetListWithOutCashWalletAsync(int? tenantId);
    }
}
