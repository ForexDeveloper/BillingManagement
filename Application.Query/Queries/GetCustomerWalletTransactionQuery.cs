using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Customers;
using Domain.Core.Enums;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries;

public class GetCustomerWalletTransactionQuery : BasePaginatedListRequest, IRequest<GetCustomerWalletTransactionsVm>
{
    public GetCustomerWalletTransactionQuery(int id, int walletId, List<TransactionType> types, int pageSize, int pageIndex = 1, int? tenantId = null)
    {
        CustomerId = id;
        WalletId = walletId;
        TenantId = tenantId;
        PageSize = pageSize;
        PageIndex = pageIndex;
        Types = types;
    }

    public List<TransactionType> Types { get; set; }

    public int? TenantId { get; set; }
    public int WalletId { get; set; }
    public int CustomerId { get; set; }

}

public class GetCustomerWalletTransactionQueryhandler : BaseQueryHandler, IRequestHandler<GetCustomerWalletTransactionQuery, GetCustomerWalletTransactionsVm>
{
    private readonly IWalletReadOnlyRepository _walletReadOnlyRepository;

    public GetCustomerWalletTransactionQueryhandler(IWalletReadOnlyRepository walletReadOnlyRepository)
    {
        _walletReadOnlyRepository = walletReadOnlyRepository;
    }

    public async Task<GetCustomerWalletTransactionsVm> Handle(GetCustomerWalletTransactionQuery request, CancellationToken cancellationToken)
    {
        var result = await _walletReadOnlyRepository.GetCustomerWalletTransactionsAsync(request, cancellationToken);

        return new GetCustomerWalletTransactionsVm
        {
            Items = result.Items.Select(p => new CustomerWalletTransactionsVm
            {
                Id = p.Id,
                Amount = p.Amount,
                TransactionTime = p.TransactionTime,
                Type = p.Type,
                Description = p.Description

            }).ToList(),
            PageIndex = result.PageIndex,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };

    }
}