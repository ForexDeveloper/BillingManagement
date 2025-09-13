using Application.Query.Queries;
using Application.Query.QueryModels;
using Application.Query.ReadOnlyRepositoryContracts;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.ReadonlyRepositories
{
    public class CustomerReadOnlyRepository : ICustomerReadOnlyRepository
    {
        private readonly ReadonlyApplicationDbContext _readonlyApplicationDbContext;
        public CustomerReadOnlyRepository(ReadonlyApplicationDbContext readonlyApplicationDbContext)
        {
            _readonlyApplicationDbContext = readonlyApplicationDbContext;
        }

        public async Task<CustomerQueryModel> GetAsync(int id, int? tenantId)
        {
            var result = await _readonlyApplicationDbContext.Customers
                .Include(c => c.Tenant)
                .FirstOrDefaultAsync(x => x.Id == id && (!tenantId.HasValue || x.TenantId == tenantId));

            if (result is null)
                return null;

            return new CustomerQueryModel(result);
        }

        public async Task<CustomersQueryModel> GetCustomerListAsync(GetCustomerListQuery query)
        {
            var queryCustomers = _readonlyApplicationDbContext.Customers
                .Include(c => c.Tenant)
                .Where(c => c.TenantId == query.TenantId);

            var totalCounts = await queryCustomers.CountAsync();
            var result = await queryCustomers
              .Skip((query.PageIndex - 1) * query.PageSize)
                .Take(query.PageSize)
               .Select(x => new CustomerQueryModel()
               {
                   Id = x.Id,
                   TenantId = x.TenantId,
                   TenantName = x.Tenant.Title,
                   FullName = x.FullName,
                   Mobile = x.Mobile,
                   NationalId = x.NationalId
               }).ToListAsync();


            return new CustomersQueryModel
            {
                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
                Items = result,
                TotalCount = totalCounts
            };
        }
    }
}
