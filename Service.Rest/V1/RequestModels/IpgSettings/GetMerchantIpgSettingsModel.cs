namespace Service.Rest.V1.RequestModels.IpgSettings;

public class GetMerchantIpgSettingsModel
{
    public int? MerchantId { get; set; }
    public bool? IsActive { get; set; }
}