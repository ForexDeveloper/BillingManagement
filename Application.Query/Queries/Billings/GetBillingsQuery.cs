using MediatR;
using System.Threading;
using Domain.Core.Enums;
using System.Threading.Tasks;
using Application.Query.Base;
using Application.Query.ViewModels.Billings;
using Application.Query.ReadOnlyRepositoryContracts;

namespace Application.Query.Queries.Billings;

public sealed class GetBillingsQuery : BasePaginatedListRequest, IRequest<GetBillingsViewModel>
{
    public string Code { get; set; }

    public int TenantId { get; set; }

    public int? MerchantId { get; set; }

    public BillingType? Type { get; set; }

    public BillingStatus? Status { get; set; }

    public GetBillingsQuery(int tenantId, int? merchantId, string? code, BillingType? type, BillingStatus? status, int pageSize, int pageIndex)
    {
        Code = code;
        Type = type;
        Status = status;
        TenantId = tenantId;
        PageSize = pageSize;
        PageIndex = pageIndex;
        MerchantId = merchantId;
    }
}

public sealed class GetBillingsQueryHandler(IB2bBillingReadOnlyRepository repository)
    : BaseQueryHandler, IRequestHandler<GetBillingsQuery, GetBillingsViewModel>
{
    public async Task<GetBillingsViewModel> Handle(GetBillingsQuery query, CancellationToken cancellationToken)
    {
        return await repository.GetBillingsAsync(query);
    }
}