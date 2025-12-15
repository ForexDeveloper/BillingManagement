using System;
using MediatR;
using System.Threading;
using Domain.Core.Enums;
using System.Threading.Tasks;
using Application.Query.Base;
using Application.Query.ViewModels.Billings;
using Application.Query.ReadOnlyRepositoryContracts;

namespace Application.Query.Queries.MerchantBilling;

public sealed class GetMerchantBillingsQuery : BasePaginatedListRequest, IRequest<GetBillingsVm>
{
    public string Code { get; set; }

    public int TenantId { get; set; }

    public int? MerchantId { get; set; }

    public BillingType? Type { get; set; }

    public BillingStatus? Status { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? DueDate { get; set; }

    public GetMerchantBillingsQuery(int tenantId, int? merchantId, string? code, BillingType? type, BillingStatus? status,
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

public sealed class GetMerchantBillingsQueryHandler(IMerchantBillingReadOnlyRepository repository)
    : BaseQueryHandler, IRequestHandler<GetMerchantBillingsQuery, GetBillingsVm>
{
    public async Task<GetBillingsVm> Handle(GetMerchantBillingsQuery query, CancellationToken cancellationToken)
    {
        return await repository.GetBillingsAsync(query);
    }
}