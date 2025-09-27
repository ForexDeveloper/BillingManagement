using Application.Query.Providers;
using Application.Query.Queries.Providers;
using Application.Query.QueryModels.Providers;
using Application.Query.ReadOnlyRepositoryContracts;
using Domain.Core.Entities.Providers;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.ReadonlyRepositories
{
    public class ProviderReadOnlyRepository : IProviderReadOnlyRepository
    {
        private readonly ReadonlyApplicationDbContext _readonlyApplicationDbContext;
        private readonly ApplicationDbContext _applicationDbContext;
        public ProviderReadOnlyRepository(ReadonlyApplicationDbContext readonlyApplicationDbContext, ApplicationDbContext applicationDbContext)
        {
            _readonlyApplicationDbContext = readonlyApplicationDbContext;
            _applicationDbContext = applicationDbContext;
        }

        public async Task<Provider> GetAsync(int id)
        {
            return await _readonlyApplicationDbContext.Providers
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<GetProvidersQueryModel> GetProvidersAsync(GetProvidersQuery request)
        {
            var query = _readonlyApplicationDbContext.Providers
                .AsQueryable();

            if (request.ProviderType.HasValue)
            {
                query = query.Where(x => x.ProviderType == request.ProviderType);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                request.SearchValue = request.SearchValue.Trim();
                query = query.Where(x =>
                       x.Name.Contains(request.SearchValue)
                    );
            }

            var totalCounts = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(request.SortColumn))
            {
                query = request.SortColumn.ToLower() switch
                {
                    "name" => request.SortDirection == Application.Query.Base.SortDirection.Ascending ?
                        query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name),
                    "type" => request.SortDirection == Application.Query.Base.SortDirection.Ascending ?
                        query.OrderBy(x => x.ProviderType) : query.OrderByDescending(x => x.ProviderType),
                    _ => query.OrderByDescending(x => x.Id)
                };
            }
            else
            {
                query = query.OrderByDescending(x => x.Id);
            }

            var providers = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                 .Take(request.PageSize)
                .Select(x => new ProviderQueryModel
                {
                    Id = x.Id,
                    ProviderType = x.ProviderType,
                    Name = x.Name,
                    Description = x.Description
                })
                .ToListAsync();

            return new GetProvidersQueryModel()
            {
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                Items = providers,
                TotalCount = totalCounts
            };
        }

    }
}
