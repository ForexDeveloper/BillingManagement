namespace Application.Query.QueryModels.TenantPlatfromContracts;

public class TenantPlatformContractSummaryQueryModel
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int TenantIpgSettingId { get; set; }
    public bool Status { get; set; }
}