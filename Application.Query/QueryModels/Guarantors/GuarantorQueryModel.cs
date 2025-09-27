namespace Application.Query.QueryModels.Guarantors;

public class GuarantorQueryModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int TenantId { get; set; }
    public string TenantName { get; set; }
    public byte PersonType { get; set; }
}
