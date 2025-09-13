using Application.Query.Queries;
using Application.Query.QueryModels;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels;
using Domain.Core.AggregateRoots.WalletConfigurationAggregate;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.Repositories
{
    public class ClosedloopReadOnlyRepository : IClosedloopReadOnlyRepository
    {
        private readonly ReadonlyApplicationDbContext _readonlyApplicationDbContext;

        public ClosedloopReadOnlyRepository(ReadonlyApplicationDbContext readonlyApplicationDbContext)
        {
            _readonlyApplicationDbContext = readonlyApplicationDbContext;
        }

        public async Task<ClosedloopQueryModel> GetByIdAsync(int id, int? tenantId = null)
        {
            var result = await _readonlyApplicationDbContext.Closedloops
                 .Where(x => x.Id == id && (!tenantId.HasValue || x.WalletConfiguration.TenantId == tenantId))
                 .Select(x => new ClosedloopQueryModel
                 {
                     Categories = x.ClosedloopCategories.Select(c => new ClosedloopCategoryQueryModel
                     {
                         CategorId = c.CategoryId,
                         CategorIdTitle = c.Category.Title,
                         Id = c.Id
                     }),
                     Merchants = x.ClosedloopMerchants.Select(c => new ClosedloopMerchantQueryModel
                     {
                         Id = c.Id,
                         MerchantId = c.MerchantId,
                         MerchantTitle = c.Merchant.Title
                     }),
                     Id = x.Id,
                     TenantId = x.WalletConfiguration.TenantId,
                     TenantTitle = x.WalletConfiguration.Tenant.Title,
                     WalletConfigurationTitle = x.WalletConfiguration.Title,
                     Title = x.Title,
                     WalletConfigurationId = x.WalletConfigurationId,
                     
                 }).FirstOrDefaultAsync();

            return result;
        }
        public async Task<GetClosedloopForGridQueryModel> GetListAsync(GetAllClosedloopQuery query)
        {
            var closedloopQuery = _readonlyApplicationDbContext.Closedloops.AsQueryable();

            if (query.TenantId > 0)
            {
                closedloopQuery = closedloopQuery.Where(c => c.WalletConfiguration.TenantId == query.TenantId);
            }
            if (query.WalletConfigurationId > 0)
            {
                closedloopQuery = closedloopQuery.Where(c => c.WalletConfigurationId == query.WalletConfigurationId);
            }
            if (!string.IsNullOrWhiteSpace(query.SearchValue))
            {
                query.SearchValue = query.SearchValue.Trim();
                closedloopQuery = closedloopQuery
                    .Where(x => x.Title.Contains(query.SearchValue));
            }
            var totalCounts = await closedloopQuery.CountAsync();

            closedloopQuery = query.SortColumn?.ToLower() switch
            {
                "title" => query.SortDirection == Application.Query.Base.SortDirection.Ascending ?
                                            closedloopQuery.OrderBy(x => x.Title) : closedloopQuery.OrderByDescending(x => x.Title),

                _ => closedloopQuery.OrderByDescending(x => x.Id)
            };


            var result = await closedloopQuery
                .Skip((query.PageIndex - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(x => new GetAllClosedloopQueryModel()
                {
                    Id = x.Id,
                    Title = x.Title,
                    WalletConfigurationId=  x.WalletConfigurationId,
                    WalletConfigurationTitle=x.WalletConfiguration.Title
                }).ToListAsync();


            return new GetClosedloopForGridQueryModel()
            {
                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
                Items = result,
                TotalCount = totalCounts
            };
        }
    }

}
