namespace Service.Rest.V1.RequestModels.Purchases;

public class CancelPurchaseModel
{
    public int PaymentId { get; set; }
    public string RefCode { get; set; } //Refrence code that i created b
}

// Need RefCode

// Update balance in account (implementation solution, just use current amounts ?)

// Do payment details need state or not ??? Ask Saro
