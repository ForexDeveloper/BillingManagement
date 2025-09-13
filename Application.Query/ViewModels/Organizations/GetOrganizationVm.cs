namespace Application.Query.ViewModels.Organizations;

public class GetOrganizationVm
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Title { get; set; }
    public int? ParentId { get; set; }
    public string ParentName { get; set; }
    public string TenantName { get; set; }
}