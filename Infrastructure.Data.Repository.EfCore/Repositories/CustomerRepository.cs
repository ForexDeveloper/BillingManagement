using Domain.Core.Entities.CustomerAggregate;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public CustomerRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task AddAsync(Customer customer)
        {
            await _applicationDbContext.Customers.AddAsync(customer);
        }

        public async Task AddRangeAsync(List<Customer> customers)
        {
            await _applicationDbContext.Customers.AddRangeAsync(customers);
        }

        public void Update(Customer customer)
        {
            _applicationDbContext.Customers.Update(customer);
        }

        public async Task<Customer> GetAsync(int id)
        {
            var customer = await _applicationDbContext.Customers
                .Include(x => x.CustomerOrganizations)
                .FirstOrDefaultAsync(x => x.Id == id);
            return customer;
        }

        public async Task<bool> CustomersBelongToOrganizationAsync(List<int> customersId, int organizationId)
        {
            var count = await _applicationDbContext.CustomerOrganizations
                .Where(p => p.OrganizationId == organizationId).CountAsync(p => customersId.Contains(p.CustomerId));

            return count == customersId.Count;
        }

        public async Task<List<Customer>> GetCustomersByIds(List<int> customersId, int? tenantId = null)
        {
            return await _applicationDbContext.Customers
                .Include(x => x.CustomerOrganizations)
                .Where(x => customersId.Contains(x.Id) && (!tenantId.HasValue || x.TenantId == tenantId))
                .ToListAsync();
        }

        public async Task<List<int>> GetCustomersIdByOrganizationId(int tenantId, int organizationId)
        {
            var customersId = await _applicationDbContext.CustomerOrganizations
                .Include(x => x.Organization)
                .Where(p => p.OrganizationId == organizationId && p.Organization.TenantId == tenantId)
                .Select(x => x.CustomerId).ToListAsync();

            return customersId;
        }
    }

}
