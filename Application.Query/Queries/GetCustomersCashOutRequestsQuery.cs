using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Customers;
using Domain.Core.Enums;
using MediatR;

namespace Application.Query.Queries;

public class GetCustomersCashOutRequestsQuery : BasePaginatedListRequest, IRequest<GetCustomerCashOutRequestsQueryVm>
{
    public GetCustomersCashOutRequestsQuery(BasePaginatedListRequest query,
        CashOutRequestStatus? status,
        DateTime? fromCreateDateTime,
        DateTime? toCreateDateTime,
        int? tenantId)
    {
        PageIndex = query.PageIndex;
        PageSize = query.PageSize;
        SortColumn = query.SortColumn;
        SortDirection = query.SortDirection;
        SearchValue = query.SearchValue;
        Status = status;
        TenantId = tenantId;
        FromCreateDateTime = fromCreateDateTime;
        ToCreateDateTime = toCreateDateTime;
    }

    public CashOutRequestStatus? Status { get; set; }
    public int? TenantId { get; set; }
    public DateTime? FromCreateDateTime { get; set; }
    public DateTime? ToCreateDateTime { get; set; }
}

public class GetCustomersCashOutRequestsQueryHandler : BaseQueryHandler, IRequestHandler<GetCustomersCashOutRequestsQuery, GetCustomerCashOutRequestsQueryVm>
{
    private readonly ICashOutRequestReadOnlyRepository _cashOutRequestReadOnlyRepository;

    public GetCustomersCashOutRequestsQueryHandler(ICashOutRequestReadOnlyRepository cashOutRequestReadOnlyRepository)
    {
        _cashOutRequestReadOnlyRepository = cashOutRequestReadOnlyRepository;
    }

    public async Task<GetCustomerCashOutRequestsQueryVm> Handle(GetCustomersCashOutRequestsQuery request, CancellationToken cancellationToken)
    {
        var result = await _cashOutRequestReadOnlyRepository.GetCustomerCashOutRequestListAsync(request);
        
        return new GetCustomerCashOutRequestsQueryVm()
        {
            PageIndex = result.PageIndex,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            Items = result.Items.Select(c => new GetCustomerCashOutRequestQueryVm
            {
                Id=c.Id,
                Status = c.Status,
                Amount = c.Amount,
                BankTransactionCode = c.BankTransactionCode,
                CreatedDateTime = c.CreatedDateTime,
                FollowUpCode = c.FollowUpCode,
                RejectReason = c.RejectReason,
                FullName = c.FullName,
                Iban = c.Iban,
                NationalCode = c.NationalCode,
                Mobile = c.Mobile,
                Description = c.Description,
                CustomerId = c.CustomerId,

            }).ToList()
            
        };
    }
}