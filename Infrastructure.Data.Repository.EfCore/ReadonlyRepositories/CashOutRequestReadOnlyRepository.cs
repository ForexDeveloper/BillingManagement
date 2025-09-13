using Application.Query.Queries;
using Application.Query.QueryModels;
using Application.Query.ReadOnlyRepositoryContracts;
using Domain.Core.Entities.CashOutRequestAggregate;
using Domain.Core.Entities.CustomerAggregate;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.ReadonlyRepositories;

public class CashOutRequestReadOnlyRepository : ICashOutRequestReadOnlyRepository
{
    private readonly ReadonlyApplicationDbContext _context;

    public CashOutRequestReadOnlyRepository(ReadonlyApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerCashOutRequestsQueryModel> GetCustomerCashOutRequestListAsync(GetCustomersCashOutRequestsQuery query)
    {
        var queryResult = _context.CashOutRequests.AsQueryable();

        if (query.TenantId.HasValue)
            queryResult = queryResult.Where(cor => cor.TenantId == query.TenantId);

        if (query.Status.HasValue)
            queryResult = queryResult.Where(cor => cor.Status == query.Status);

        if (query.FromCreateDateTime.HasValue)
            queryResult = queryResult.Where(cor => cor.CreatedDateTime.Date >= query.FromCreateDateTime.Value.Date);

        if (query.ToCreateDateTime.HasValue)
            queryResult = queryResult.Where(cor => cor.CreatedDateTime.Date <= query.ToCreateDateTime.Value.Date);

        if (!string.IsNullOrWhiteSpace(query.SearchValue))
            queryResult = FilterSearchValue(query, queryResult);

        if (!string.IsNullOrWhiteSpace(query.SortColumn))
        {
            queryResult = query.SortColumn.ToLower() switch
            {
                "bankTransactionCode" => query.SortDirection == Application.Query.Base.SortDirection.Ascending ?
                    queryResult.OrderBy(x => x.BankTransactionCode) : queryResult.OrderByDescending(x => x.BankTransactionCode),
                _ => queryResult.OrderByDescending(x => x.Id)
            };
        }
        else
        {
            queryResult = queryResult.OrderByDescending(x => x.Id);
        }

        var totalCounts = await queryResult.CountAsync();

        var result = await queryResult
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(cor => new CustomerCashOutRequestQueryModel()
            {
                Id = cor.Id,
                Status = cor.Status,
                Amount = cor.FinancialDocument.Amount,
                FollowUpCode = cor.FollowUpCode,
                BankTransactionCode = cor.BankTransactionCode,
                CreatedDateTime = cor.CreatedDateTime,
                FullName = ((Customer)cor.CashWallet.BusinessIdentity).FullName,
                Iban = cor.BankAccount.Iban,
                RejectReason = cor.RejectReason,
                NationalCode = ((Customer)cor.CashWallet.BusinessIdentity).NationalId,
                Mobile = ((Customer)cor.CashWallet.BusinessIdentity).Mobile,
                Description = cor.Description,
                CustomerId = cor.CashWallet.BusinessIdentityId
            }).ToListAsync();

        return new CustomerCashOutRequestsQueryModel()
        {
            PageIndex = query.PageIndex,
            PageSize = query.PageSize,
            Items = result,
            TotalCount = totalCounts
        };
    }

    private static IQueryable<CashOutRequest> FilterSearchValue(GetCustomersCashOutRequestsQuery query, IQueryable<CashOutRequest> queryResult)
    {
        var searchValue = query.SearchValue;

        if (!string.IsNullOrEmpty(searchValue))
        {
            queryResult = queryResult.Where(cor =>
                ((Customer)cor.CashWallet.BusinessIdentity).Mobile.Contains(searchValue) ||
                ((Customer)cor.CashWallet.BusinessIdentity).NationalId.Contains(searchValue) ||
                ((Customer)cor.CashWallet.BusinessIdentity).FullName.Contains(searchValue) ||
                cor.BankAccount.Iban.Contains(searchValue) || cor.FollowUpCode.ToString().Contains(searchValue));
        }

        return queryResult;
    }
}