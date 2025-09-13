using Application.Query.Queries;
using Application.Query.QueryModels;
using System.Threading.Tasks;

namespace Application.Query.ReadOnlyRepositoryContracts
{
    public interface ICustomerReadOnlyRepository
    {
        Task<CustomerQueryModel> GetAsync(int id, int? tenantId = null);
        Task<CustomersQueryModel> GetCustomerListAsync(GetCustomerListQuery request);
    }
}
