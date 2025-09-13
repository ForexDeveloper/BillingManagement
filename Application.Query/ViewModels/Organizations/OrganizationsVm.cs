using System.Collections.Generic;

namespace Application.Query.ViewModels.Organizations;

public class OrganizationsVm
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int? ParentId { get; set; }
    public string ParentName { get; set; }
    public List<OrganizationsVm> Children { get; set; }
}