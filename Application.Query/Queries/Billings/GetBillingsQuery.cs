using System;
using MediatR;
using System.Threading;
using Domain.Core.Enums;
using System.Threading.Tasks;
using Application.Query.Base;
using Application.Query.ViewModels.Billings;
using Application.Query.ReadOnlyRepositoryContracts;

namespace Application.Query.Queries.Billings;

public sealed class GetBillingsQuery : BasePaginatedListRequest, IRequest<GetBillingsVm>
{
    public string Code { get; set; }

    public int TenantId { get; set; }

    public int? MerchantId { get; set; }

    public BillingType? Type { get; set; }

    public BillingStatus? Status { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? DueDate { get; set; }

    public GetBillingsQuery(int tenantId, int? merchantId, string? code, BillingType? type, BillingStatus? status,
        DateTime? startDate, DateTime? dueDate, int pageSize, int pageIndex)
    {
        Code = code;
        Type = type;
        Status = status;
        DueDate = dueDate;
        TenantId = tenantId;
        PageSize = pageSize;
        PageIndex = pageIndex;
        StartDate = startDate;
        MerchantId = merchantId;
    }
}

public sealed class GetBillingsQueryHandler(IBillingReadOnlyRepository repository)
    : BaseQueryHandler, IRequestHandler<GetBillingsQuery, GetBillingsVm>
{
    public async Task<GetBillingsVm> Handle(GetBillingsQuery query, CancellationToken cancellationToken)
    {
        return await repository.GetBillingsAsync(query);
    }
}