namespace Service.Rest.V1.RequestModels.Organizations
{
    public class CreateTenantOrganizationModel
    {
        public string Title { get; set; }
        public string? BusinessType { get; set; }
        public int? ParentId { get; set; }
    }
}
