namespace Service.Rest.V1.RequestModels.Customers;

public class CreateCashOutRequestModel
{
    public int BankAccountId { get; set; }
    public decimal Amount { get; set; }
}