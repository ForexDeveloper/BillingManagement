using Application.Query.QueryModels;
using Application.Query.ReadOnlyRepositoryContracts;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;
using Application.Query.QueryModels.IpgSettings;
using Application.Query.QueryModels.Tenants;
using Application.Query.Queries.IpgSettings;
using Application.Query.Queries.Tenants;

namespace Infrastructure.Data.Repository.EfCore.ReadonlyRepositories
{
    public class TenantReadOnlyRepository : ITenantReadOnlyRepository
    {
        private readonly ReadonlyApplicationDbContext _readonlyApplicationDbContext;

        public TenantReadOnlyRepository(ReadonlyApplicationDbContext readonlyApplicationDbContext)
        {
            _readonlyApplicationDbContext = readonlyApplicationDbContext;
        }

        public async Task<Tenant> GetAsync(int id)
        {
            var tenant = await _readonlyApplicationDbContext.Tenants.FirstOrDefaultAsync(x => x.Id == id);

            return tenant;
        }

        public async Task<TenantIpgSetting> TenantIpSettingGetAsync(int id, int? tenantId = null)
        {
            var tenant = await _readonlyApplicationDbContext.TenantIpgSettings
                .Include(x => x.Tenant)
                .FirstOrDefaultAsync(x => x.Id == id && (!tenantId.HasValue || x.TenantId == tenantId));

            return tenant;
        }

        public async Task<GetTenantIpgSettingQueryModel> TenantIpSettingGetAllAsync(GetTenantIpgSettingsQuery request, int platformTenantId)
        {
            var query = _readonlyApplicationDbContext.TenantIpgSettings
                .Include(x => x.Tenant)
                .AsQueryable();

            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive);
            }

            if (request.TenantId.HasValue && request.IpgSettingOwnerType is IpgSettingOwnerType.Tenant)
            {
                query = query.Where(x => x.TenantId == request.TenantId);
            }
            else
            {
                query = query.Where(x => x.TenantId == platformTenantId);
            }

            var totalCounts = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(request.SortColumn))
            {
                query = request.SortColumn.ToLower() switch
                {
                    "title" => request.SortDirection == Application.Query.Base.SortDirection.Ascending ?
                        query.OrderBy(x => x.Title) : query.OrderByDescending(x => x.Title),
                    _ => query.OrderByDescending(x => x.Id)
                };
            }
            else
            {
                query = query.OrderByDescending(x => x.Id);
            }

            var settings = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new TenantIpgSettingQueryModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    TenantId = x.TenantId,
                    TenantName = x.Tenant.Title,
                    IpgType = x.IpgType,
                    IsActive = x.IsActive,
                })
                .ToListAsync();


            return new GetTenantIpgSettingQueryModel()
            {
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                Items = settings,
                TotalCount = totalCounts
            };
        }


        public async Task<GetTenantsQueryModel> GetTenantsAsync(GetTenantsQuery request)
        {
            var query = _readonlyApplicationDbContext.Tenants.Where(x => x.Id != 1)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                request.SearchValue = request.SearchValue.Trim();
                query = query.Where(x => x.Title.Contains(request.SearchValue));
            }

            var totalCounts = await query.CountAsync();

            query = query.OrderByDescending(x => x.Id);

            var tenants = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new TenantQueryModel()
                {
                    Id = x.Id,
                    Title = x.Title,
                })
                .ToListAsync();

            return new GetTenantsQueryModel()
            {
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                Items = tenants,
                TotalCount = totalCounts
            };
        }

    }

}
