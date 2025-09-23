using Domain.Core.Enums;

namespace Service.Rest.V1.RequestModels.Billings;
public class UpdateBillingForTestRequest
{
    public int WalletId { get; set; }
    public TimeInterval TimeInterval { get; set; }
    public int Time { get; set; }
}
