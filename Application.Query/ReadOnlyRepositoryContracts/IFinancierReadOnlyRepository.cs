using Application.Query.Queries.Financiers;
using Application.Query.QueryModels.Financiers;
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
