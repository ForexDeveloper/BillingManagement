using System.Collections.Generic;

namespace Application.Query.QueryModels;

public class CustomerOrganizationQueryModel
{
    public int CustomerId { get; set; }
    public List<int> OrganizationsId { get; set; }
    public CustomerOrganizationQueryModel()
    {

    }

    public CustomerOrganizationQueryModel(int customerId, List<int> organizationsId)
    {
        CustomerId = customerId;
        OrganizationsId = organizationsId;
    }
}


