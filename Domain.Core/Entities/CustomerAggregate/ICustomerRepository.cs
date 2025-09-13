using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Core.Entities.CustomerAggregate
{
    public interface ICustomerRepository
    {
        Task AddAsync(Customer customer);
        Task AddRangeAsync(List<Customer> customers);
        void Update(Customer customer);
        Task<Customer> GetAsync(int id);
        Task<bool> CustomersBelongToOrganizationAsync(List<int> customersId, int organizationId);
        Task<List<Customer>> GetCustomersByIds(List<int> customersId, int? tenantId = null);
        Task<List<int>> GetCustomersIdByOrganizationId(int tenantId, int organizationId);
    }
}