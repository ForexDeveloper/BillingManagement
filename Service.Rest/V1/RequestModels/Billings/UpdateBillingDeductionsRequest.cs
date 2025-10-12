namespace Service.Rest.V1.RequestModels.Billings;

public sealed record UpdateBillingDeductionsRequest
{
    public decimal Amount { get; set; }

    public string? Description { get; set; }
}