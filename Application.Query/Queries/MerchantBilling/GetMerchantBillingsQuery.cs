using MediatR;
using System.Threading;
using Domain.Core.Enums;
using System.Threading.Tasks;
using Application.Query.Base;
using Application.Query.ViewModels.Billings;
using Application.Query.ReadOnlyRepositoryContracts;

namespace Application.Query.Queries.MerchantBilling;

public sealed class GetMerchantBillingsQuery : BasePaginatedListRequest, IRequest<GetBillingsViewModel>
{
    public string Code { get; set; }

    public int TenantId { get; set; }

    public int? MerchantId { get; set; }

    public BillingStatus? Status { get; set; }

    public GetMerchantBillingsQuery(int tenantId, int merchantId, string? code, BillingStatus? status, int pageSize, int pageIndex)
    {
        Code = code;
        Status = status;
        TenantId = tenantId;
        PageSize = pageSize;
        PageIndex = pageIndex;
        MerchantId = merchantId;
    }
}

public sealed class GetMerchantBillingsQueryHandler(IMerchantBillingReadOnlyRepository repository)
    : BaseQueryHandler, IRequestHandler<GetMerchantBillingsQuery, GetBillingsViewModel>
{
    public async Task<GetBillingsViewModel> Handle(GetMerchantBillingsQuery query, CancellationToken cancellationToken)
    {
        return await repository.GetBillingsAsync(query);
    }
}