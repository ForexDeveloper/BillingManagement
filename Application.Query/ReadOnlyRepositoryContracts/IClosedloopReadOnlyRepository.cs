using Application.Query.Queries;
using Application.Query.QueryModels;
using Application.Query.ViewModels;
using System.Threading.Tasks;

namespace Application.Query.ReadOnlyRepositoryContracts
{
    public interface IClosedloopReadOnlyRepository
    {
        Task<ClosedloopQueryModel> GetByIdAsync(int id, int? tenantId = null);
        Task<GetClosedloopForGridQueryModel> GetListAsync(GetAllClosedloopQuery query);
    }
}
