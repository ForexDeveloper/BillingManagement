namespace Application.Query.ViewModels.IpgSettings;

public class TenantIpgSettingVm
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int? TenantId { get; set; }
    public string TenantName { get; set; }
    public byte IpgType { get; set; }
    public string IpgTypeTitle { get; set; }
    public bool IsActive { get; set; }
}