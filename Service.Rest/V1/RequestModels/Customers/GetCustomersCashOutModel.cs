using Application.Query.Base;
using Domain.Core.Enums;

namespace Service.Rest.V1.RequestModels.Customers;

public class GetCustomersCashOutModel : BasePaginatedListRequest
{
    public CashOutRequestStatus? Status { get; set; }
    public DateTime? FromCreateDateTime { get; set; }
    public DateTime? ToCreateDateTime { get; set; }
}

public class AdminGetCustomersCashOutModel : GetCustomersCashOutModel
{
    public int TenantId { get; set; }
}
