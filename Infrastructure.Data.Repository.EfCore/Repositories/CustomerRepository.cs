using Domain.Core.Entities.CustomerAggregate;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;
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

        public async Task<List<Customer>> GetCustomersByIds(List<int> customersId, int? tenantId = null)
        {
            return await _applicationDbContext.Customers
                .Include(x => x.CustomerOrganizations)
                .Where(x => customersId.Contains(x.Id) && (!tenantId.HasValue || x.TenantId == tenantId))
                .ToListAsync();
        }
    }

}
