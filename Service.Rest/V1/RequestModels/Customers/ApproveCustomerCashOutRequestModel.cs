namespace Service.Rest.V1.RequestModels.Customers;

public class ApproveCustomerCashOutRequestModel
{
    public string BankTransactionCode { get; set; }

    public string Description { get; set; }
}