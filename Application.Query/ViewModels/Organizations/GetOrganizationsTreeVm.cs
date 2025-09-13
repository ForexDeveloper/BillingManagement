using System.Collections.Generic;

namespace Application.Query.ViewModels.Organizations;

public class GetOrganizationsTreeVm
{
    public List<OrganizationsVm> Organizations { get; set; } = [];
}