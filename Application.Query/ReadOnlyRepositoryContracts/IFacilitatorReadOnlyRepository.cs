using Application.Query.Queries.Facilitators;
using Application.Query.QueryModels.Facilitators;
using Domain.Core.Entities.FacilitatorAggregate;
using System.Threading.Tasks;

namespace Application.Query.ReadOnlyRepositoryContracts;

public interface IFacilitatorReadOnlyRepository
{
    Task<Facilitator> GetAsync(int id, int? tenantId = null);
    Task<GetFacilitatorsQueryModel> GetFacilitatorsAsync(GetFacilitatorsQuery request);
}
