using Application.Query.Queries;
using Application.Query.QueryModels;
using Domain.Core.Entities.FinancierAggregate;
using System.Threading.Tasks;

namespace Application.Query.ReadOnlyRepositoryContracts
{
    public interface IFinancierReadOnlyRepository
    {
        Task<Financier> GetAsync(int id, int? tenantId = null);
        Task<GetFinanciersQueryModel> GetFinanciersAsync(GetFinanciersQuery request);
    }
}
