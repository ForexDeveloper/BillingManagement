using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Core.Entities.Providers
{
    public interface IProviderRepository
    {
        Task AddAsync(Provider provider);
        void Update(Provider provider);
        Task<Provider> GetAsync(int id);
        Task<bool> IsProviderExistsAsync(List<int> providersId);
        Task<List<Provider>> GetProvidersAsync(List<int> providerds);
    }
}