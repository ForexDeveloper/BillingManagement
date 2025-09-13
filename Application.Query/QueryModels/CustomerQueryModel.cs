using Domain.Core.Entities.CustomerAggregate;

namespace Application.Query.QueryModels;

public class CustomerQueryModel
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string TenantName { get; set; }
    public string FullName { get; set; }
    public string NationalId { get; set; }
    public string Mobile { get; set; }
    public CustomerQueryModel()
    {

    }

    public CustomerQueryModel(Customer customer)
    {
        Id = customer.Id;
        TenantId = customer.TenantId;
        TenantName = customer.Tenant.Title;
        FullName = customer.FullName;
        Mobile = customer.Mobile;
        NationalId = customer.NationalId;
    }
}


