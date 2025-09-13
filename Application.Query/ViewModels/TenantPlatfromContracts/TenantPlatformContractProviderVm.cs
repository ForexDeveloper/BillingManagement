using Domain.Core.Enums;

namespace Application.Query.ViewModels.TenantPlatfromContracts;

public class TenantPlatformContractProviderVm
{
    public int ProviderId { get; set; }
    public ProviderType ProviderType { get; set; }
    public string ProviderTypeTitle { get; set; }
    public string ProviderName { get; set; }
    public string ProviderEnglishName { get; set; }
    public decimal Amount { get; set; }
}
