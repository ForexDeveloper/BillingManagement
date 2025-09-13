namespace Application.Query.QueryModels;

public class TenantIpgSettingQueryModel
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int? TenantId { get; set; }
    public string? TenantName { get; set; }
    public byte IpgType { get; set; }
    public string IpgTypeTitle { get; set; }
    public bool IsActive { get; set; }
}