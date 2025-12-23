using System.Linq;
using Domain.Core.Enums;
using System.Threading.Tasks;
using Shared.Utilities.Extensions;
using Microsoft.EntityFrameworkCore;
using Application.Query.Queries.Billings;
using Application.Query.ViewModels.Billings;
using Application.Query.ReadOnlyRepositoryContracts;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;

namespace Infrastructure.Data.Repository.EfCore.ReadonlyRepositories;

public sealed class BillingReadOnlyRepository(ReadonlyApplicationDbContext dbContext) : IBillingReadOnlyRepository
{
    public async Task<GetBillingsVm> GetBillingsAsync(GetBillingsQuery query)
    {
        var billingQuery = dbContext.Billings.Where(p => p.TenantId == query.TenantId);

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
            billingQuery = billingQuery.Where(p => p.Code.Contains(query.Code.Trim()));
        }

        if (query.MerchantId.HasValue)
        {
            billingQuery = billingQuery.Where(p => (p.ToBusinessIdentityId == query.MerchantId && p.Type == BillingType.TenantToMerchant) || 
                                                   (p.FromBusinessIdentityId == query.MerchantId && p.Type == BillingType.MerchantToTenant));
        }

        if (query.FromDate.HasValue)
        {
            billingQuery = billingQuery.Where(p => p.DueDate >= query.FromDate);
        }

        if (query.ToDate.HasValue)
        {
            billingQuery = billingQuery.Where(p => p.DueDate <= query.ToDate);
        }

        var totalCount = await billingQuery.CountAsync();

        var billings = await billingQuery.Select(p => new GetBillingsItemVm
        {
            Id = p.Id,
            Code = p.Code,
            Type = p.Type,
            Status = p.Status,
            DueDate = p.DueDate,
            TypeTitle = p.Type.GetEnumDescription(),
            StatusTitle = p.Status.GetEnumDescription(),
            PaymentDeadlineDate = p.PaymentDeadlineDate,
            PayableAmount = p.Amount - p.Payments.Sum(q => q.Amount),
            MerchantTitle = dbContext.Merchants.FirstOrDefault(q => (q.Id == p.ToBusinessIdentityId && p.Type == BillingType.TenantToMerchant) ||
                                                                    (q.Id == p.FromBusinessIdentityId && p.Type == BillingType.MerchantToTenant)).Title
        })
        .OrderByDescending(p => p.DueDate)
        .Skip((query.PageIndex - 1) * query.PageSize)
        .Take(query.PageSize)
        .ToListAsync();

        return new GetBillingsVm
        {
            Items = billings,
            TotalCount = totalCount,
            PageSize = query.PageSize,
            PageIndex = query.PageIndex
        };
    }
}