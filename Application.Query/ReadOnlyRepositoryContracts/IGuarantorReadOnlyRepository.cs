using Application.Query.Queries;
using Application.Query.QueryModels;
using Domain.Core.Entities.GuarantorAggregate;
using System.Threading.Tasks;

namespace Application.Query.ReadOnlyRepositoryContracts;

public interface IGuarantorReadOnlyRepository
{
    Task<Guarantor> GetAsync(int id, int? tenantId = null);
    Task<GetGuarantorsQueryModel> GetGuarantorsAsync(GetGuarantorsQuery request);
}
