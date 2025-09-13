namespace Application.Query.QueryModels;

public class MerchantIpgSettingQueryModel
{
    public int Id { get; set; }
    public int? MerchantId { get; set; }
    public string? MerchantName { get; set; }
    public byte IpgType { get; set; }
    public string IpgTypeTitle { get; set; }
    public bool IsActive { get; set; }
}