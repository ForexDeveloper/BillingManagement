using Application.Query.Queries;
using Application.Query.QueryModels;
using Application.Query.ReadOnlyRepositoryContracts;
using Domain.Core.Entities.FacilitatorAggregate;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;

namespace Infrastructure.Data.Repository.EfCore.ReadonlyRepositories;

public class FacilitatorReadOnlyRepository : IFacilitatorReadOnlyRepository
{
    private readonly ReadonlyApplicationDbContext _readonlyApplicationDbContext;
    private readonly ApplicationDbContext _applicationDbContext;

    public FacilitatorReadOnlyRepository(ReadonlyApplicationDbContext readonlyApplicationDbContext, ApplicationDbContext applicationDbContext)
    {
        _readonlyApplicationDbContext = readonlyApplicationDbContext;
        _applicationDbContext = applicationDbContext;
    }

    public async Task<Facilitator> GetAsync(int id, int? tenantId = null)
    {
        return await _readonlyApplicationDbContext.Facilitators
            .Include(c => c.Tenant)
            .FirstOrDefaultAsync(x => x.Id == id);
    }


    public async Task<GetFacilitatorsQueryModel> GetFacilitatorsAsync(GetFacilitatorsQuery request)
    {
        var query = _readonlyApplicationDbContext.Facilitators
            .Include(c => c.Tenant)
            .AsQueryable();

        if (request.PersonType.HasValue)
        {
            query = query.Where(x => x.Type == (byte)request.PersonType);
        }


        if (request.TenantId.HasValue)
        {
            query = query.Where(x => x.TenantId == request.TenantId);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchValue))
        {
            request.SearchValue = request.SearchValue.Trim();
            query = query.Where(x =>
                   x.Name.Contains(request.SearchValue) ||
                   x.Tenant.Title.Contains(request.SearchValue)
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
                    query.OrderBy(x => x.Type) : query.OrderByDescending(x => x.Type),
                _ => query.OrderByDescending(x => x.Id)
            };
        }
        else
        {
            query = query.OrderByDescending(x => x.Id);
        }

        var facilitators = await query
            .Skip((request.PageIndex - 1) * request.PageSize)
             .Take(request.PageSize)
            .Select(x => new FacilitatorQueryModel
            {
                Id = x.Id,
                Name = x.Name,
                TenantId = x.TenantId,
                TenantName = x.Tenant.Title,
                PersonType = x.Type,
            })
            .ToListAsync();

        return new GetFacilitatorsQueryModel()
        {
            PageIndex = request.PageIndex,
            PageSize = request.PageSize,
            Items = facilitators,
            TotalCount = totalCounts
        };
    }
}
