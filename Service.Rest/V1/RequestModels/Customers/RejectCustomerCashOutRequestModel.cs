using Domain.Core.Enums;

namespace Service.Rest.V1.RequestModels.Customers;

public class RejectCustomerCashOutRequestModel
{
    public string Description { get; set; }

    public CashOutRequestRejectReason RejectReason { get; set; }
}