using Application.Query.Base;
using Application.Query.ViewModels.Customers.Wallets;

namespace Service.Rest.V1.RequestModels.Customers;

public class GetCustomerBillsListModel : BasePaginatedListRequest
{
    public BillDateType DateType { get; set; }
    public int? WalletId { get; set; }
}
