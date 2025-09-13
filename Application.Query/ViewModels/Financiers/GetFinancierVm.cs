using Domain.Core.Enums;

namespace Application.Query.ViewModels.Financiers;

public class GetFinancierVm
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int TenantId { get; set; }
    public string TenantName { get; set; }
    public IdentityTypeEnum PersonType { get; set; }
    public string PersonTypeName { get; set; }
}


