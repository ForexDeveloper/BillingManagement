using System;

namespace Application.Query.ViewModels.TenantPlatfromContracts;

public class TenantPlatformContractsVm
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string TenantName { get; set; }
    public string CreditProjectName { get; set; }
    public string BrandName { get; set; }
    public string InternalProjectManagerName { get; set; }
    public string ContractNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool Status { get; set; }
    public bool IsEditable { get; set; }
}