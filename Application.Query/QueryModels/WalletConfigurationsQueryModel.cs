using Domain.Core.Enums;

namespace Application.Query.ViewModels;

public class WalletConfigurationsQueryModel
{   
    public int Id { get; set; }
    public string Title { get; set; }
    public WalletType WalletTypeId { get; set; }
    public decimal MaxWallet { get; set; }
    public int TenantId { get; set; }
    public string TenantTitle { get; set; }
    public int? ProjectManagerId { get; set; }
    public string ProjectManagerFullName { get; set; }
    public int PlanCount { get; set; }
}