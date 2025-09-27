using Application.Query.Queries.Guarantors;
using Application.Query.QueryModels.Guarantors;
using Domain.Core.Entities.GuarantorAggregate;
using System.Threading.Tasks;

namespace Application.Query.ReadOnlyRepositoryContracts;

public interface IGuarantorReadOnlyRepository
{
    Task<Guarantor> GetAsync(int id, int? tenantId = null);
    Task<GetGuarantorsQueryModel> GetGuarantorsAsync(GetGuarantorsQuery request);
}
