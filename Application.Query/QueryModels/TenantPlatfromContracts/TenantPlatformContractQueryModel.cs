using System;

namespace Application.Query.QueryModels.TenantPlatfromContracts;

public class TenantPlatformContractQueryModel
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string TenantIName { get; set; }
    public string ContractNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string CreditProjectName { get; set; }
    public string BrandName { get; set; }
    public string ProjectManagerName { get; set; }
    public bool Status { get; set; }
}