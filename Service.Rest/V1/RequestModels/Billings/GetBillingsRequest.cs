using Application.Query.Base;
using Domain.Core.Enums;

namespace Service.Rest.V1.RequestModels.Billings;

public sealed class GetBillingsRequest : BasePaginatedListRequest
{
    public string Code { get; set; }

    public int? MerchantId { get; set; }

    public BillingType? Type { get; set; }

    public BillingStatus? Status { get; set; }
}