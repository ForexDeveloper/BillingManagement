namespace Service.Rest.V1.RequestModels.Transactions;

public class VerifyTransactionModel
{
    public int PaymentId { get; set; }
    public string RefrenceCode { get; set; }
}
