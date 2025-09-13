using Application.Query.Queries;
using Application.Query.QueryModels;
using Domain.Core.Entities.FacilitatorAggregate;
using System.Threading.Tasks;

namespace Application.Query.ReadOnlyRepositoryContracts;

public interface IFacilitatorReadOnlyRepository
{
    Task<Facilitator> GetAsync(int id, int? tenantId = null);
    Task<GetFacilitatorsQueryModel> GetFacilitatorsAsync(GetFacilitatorsQuery request);
}
