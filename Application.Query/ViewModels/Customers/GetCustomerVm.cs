namespace Application.Query.ViewModels.Customers
{
    public class GetCustomerVm
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string TenantName { get; set; }
        public string FullName { get; set; }
        public string NationalId { get; set; }
        public string Mobile { get; set; }
    }
}
