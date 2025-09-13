using Domain.Core.Entities.OrganizationAggregate;

namespace Application.Query.QueryModels;

public class OrganizationQueryModel
{
    public OrganizationQueryModel()
    {

    }

    public OrganizationQueryModel(Organization organization)
    {
        Id = organization.Id;
        Title = organization.Title;
        ParentId = organization.ParentId;
        TenantId = organization.TenantId;
        TenantName = organization.Tenant.Title;
    }

    public int Id { get; set; }
    public string Title { get; set; }
    public int? ParentId { get; set; }
    public string ParentName { get; set; }
    public int TenantId { get; set; }
    public string TenantName { get; set; }
}
