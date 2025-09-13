using Application.Query.Queries;
using Application.Query.QueryModels;
using Application.Query.ReadOnlyRepositoryContracts;
using Domain.Core.Entities.GuarantorAggregate;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.ReadonlyRepositories;

public class GuarantorReadOnlyRepository : IGuarantorReadOnlyRepository
{
    private readonly ReadonlyApplicationDbContext _readonlyApplicationDbContext;

    public GuarantorReadOnlyRepository(ReadonlyApplicationDbContext readonlyApplicationDbContext)
    {
        _readonlyApplicationDbContext = readonlyApplicationDbContext;
    }

    public async Task<Guarantor> GetAsync(int id, int? tenantId = null)
    {
        return await _readonlyApplicationDbContext.Guarantors
            .Include(c => c.Tenant)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<GetGuarantorsQueryModel> GetGuarantorsAsync(GetGuarantorsQuery request)
    {
        var query = _readonlyApplicationDbContext.Guarantors
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
                   x.Tenant.Title.Contains(request.SearchValue));
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

        var guarantor = await query
            .Skip((request.PageIndex - 1) * request.PageSize)
             .Take(request.PageSize)
            .Select(x => new GuarantorQueryModel
            {
                Id = x.Id,
                Name = x.Name,
                TenantId = x.TenantId,
                TenantName = x.Tenant.Title,
                PersonType = x.Type,
            })
            .ToListAsync();

        return new GetGuarantorsQueryModel()
        {
            PageIndex = request.PageIndex,
            PageSize = request.PageSize,
            Items = guarantor,
            TotalCount = totalCounts
        };
    }

}
