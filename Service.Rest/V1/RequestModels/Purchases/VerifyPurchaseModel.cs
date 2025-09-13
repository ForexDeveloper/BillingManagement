namespace Service.Rest.V1.RequestModels.Purchases;

//Do we need to check validation?

public class VerifyPurchaseModel
{
    public int PaymentId { get; set; }
    public string RefrenceCode { get; set; }
}
