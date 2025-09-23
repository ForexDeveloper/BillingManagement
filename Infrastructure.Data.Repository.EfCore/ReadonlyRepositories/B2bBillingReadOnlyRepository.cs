using Application.Query.Queries.Billings;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Billings;
using Microsoft.EntityFrameworkCore;
using Shared.Utilities.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.ReadonlyRepositories;

public sealed class B2bBillingReadOnlyRepository(ReadonlyApplicationDbContext dbContext) : IB2bBillingReadOnlyRepository
{
    public async Task<GetBillingsViewModel> GetBillingsAsync(GetBillingsQuery query)
    {
        var billingQuery = dbContext.B2bBillings.Where(p => p.FromBusinessIdentityId == query.TenantId);

        if (query.Type.HasValue)
        {
            billingQuery = billingQuery.Where(p => p.Type == query.Type);
        }

        if (query.Status.HasValue)
        {
            billingQuery = billingQuery.Where(p => p.Status == query.Status);
        }

        if (!string.IsNullOrEmpty(query.Code))
        {
            billingQuery = billingQuery.Where(p => p.Code.Contains(query.Code));
        }

        if (query.MerchantId.HasValue)
        {
            billingQuery = billingQuery.Where(p => p.ToBusinessIdentityId == query.MerchantId);
        }

        var totalCount = await billingQuery.CountAsync();

        var billings = await billingQuery.Select(p => new GetBillingsItemViewModel
        {
            Id = p.Id,
            Code = p.Code,
            Type = p.Type,
            Status = p.Status,
            DueDate = p.DueDate,
            EndDate = p.EndDate,
            TypeTitle = p.Type.GetEnumDescription(),
            StatusTitle = p.Status.GetEnumDescription(),
            PayableAmount = p.Amount - p.Payments.Sum(q => q.Amount)
        })
        .Skip((query.PageIndex - 1) * query.PageSize)
        .Take(query.PageSize)
        .ToListAsync();

        return new GetBillingsViewModel
        {
            Items = billings,
            TotalCount = totalCount,
            PageSize = query.PageSize,
            PageIndex = query.PageIndex
        };
    }
}