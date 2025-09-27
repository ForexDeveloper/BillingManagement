namespace Application.Query.QueryModels.Financiers;

public class FinancierQueryModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int TenantId { get; set; }
    public string TenantName { get; set; }
    public byte PersonType { get; set; }

    public FinancierQueryModel() { }

}
