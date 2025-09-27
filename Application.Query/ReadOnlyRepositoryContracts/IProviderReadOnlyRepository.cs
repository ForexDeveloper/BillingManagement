using Application.Query.Providers;
using Application.Query.Queries.Providers;
using Domain.Core.Entities.Providers;
using System.Threading.Tasks;

namespace Application.Query.ReadOnlyRepositoryContracts
{
    public interface IProviderReadOnlyRepository
    {
        Task<Provider> GetAsync(int id);
        Task<GetProvidersQueryModel> GetProvidersAsync(GetProvidersQuery request);
    }
}
