using Application.Query.Queries;
using Application.Query.QueryModels;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Organizations;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.ReadonlyRepositories
{
    public class OrganizationReadOnlyRepository : IOrganizationReadOnlyRepository
    {
        private readonly ReadonlyApplicationDbContext _readonlyApplicationDbContext;
        public OrganizationReadOnlyRepository(ReadonlyApplicationDbContext readonlyApplicationDbContext)
        {
            _readonlyApplicationDbContext = readonlyApplicationDbContext;
        }

        public async Task<OrganizationQueryModel> GetByIdAsync(int id, int? tenantId)
        {
            var dataModel = await _readonlyApplicationDbContext.Organizations
                .Include(x => x.Parent)
                .Include(x => x.Tenant)
                .FirstOrDefaultAsync(x => x.Id == id && (!tenantId.HasValue || x.TenantId == tenantId));

            return dataModel == null ? null :
                new OrganizationQueryModel()
                {
                    Id = dataModel.Id,
                    TenantId = dataModel.TenantId,
                    Title = dataModel.Title,
                    ParentId = dataModel.ParentId,
                    ParentName = dataModel.ParentId != null ? dataModel.Parent.Title : null,
                    TenantName = dataModel.Tenant.Title,
                };

        }

        public async Task<List<OrganizationsVm>> GetByTenantIdAsync(int tenantId)
        {
            var dataModel = await _readonlyApplicationDbContext.Organizations
                .Include(x => x.Parent)
                .Include(x => x.Tenant)
                .Where(x => x.TenantId == tenantId).ToListAsync();

            return dataModel.Select(item => new OrganizationsVm
            {
                Id = item.Id,
                Title = item.Title,
                ParentId = item.ParentId,
                ParentName = item.ParentId != null ? item.Parent.Title : null,
                Children = []
            }).ToList();
        }

        public async Task<GetOrganizationsQueryModel> GetOrganizationsAsync(GetOrganizationsQuery request)
        {
            var query = _readonlyApplicationDbContext.Organizations
                .Include(x => x.Parent)
                .Include(x => x.Tenant)
                .AsQueryable();

            if (request.TenantId.HasValue)
            {
                query = query.Where(x => x.TenantId == request.TenantId);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                request.SearchValue = request.SearchValue.Trim();
                query = query.Where(x =>
                       x.Title.Contains(request.SearchValue) ||
                       x.Tenant.Title.Contains(request.SearchValue)
                    );
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

            var organizations = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                 .Take(request.PageSize)
                .Select(x => new OrganizationQueryModel
                {
                    Id = x.Id,
                    TenantId = x.TenantId,
                    Title = x.Title,
                    ParentId = x.ParentId,
                    ParentName = x.ParentId != null ? x.Parent.Title : null,
                    TenantName = x.Tenant.Title,
                })
                .ToListAsync();

            return new GetOrganizationsQueryModel()
            {
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                Items = organizations,
                TotalCount = totalCounts
            };
        }
    }
}
