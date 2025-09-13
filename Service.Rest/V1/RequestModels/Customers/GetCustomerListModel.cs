using Application.Query.Base;

namespace Service.Rest.V1.RequestModels.Customers
{
    public class GetCustomerListModel : BasePaginatedListRequest
    {
        public int TenantId { get; set; }

    }
    public class GetCustomerListBaseModel : BasePaginatedListRequest
    {

    }
}
