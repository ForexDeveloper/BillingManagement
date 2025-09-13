using Application.Service.Dtos.WalletContract;

namespace Service.Rest.V1.RequestModels.WalletContracts
{
    public class CreateWalletContractModel : CreateWalletContractBaseModel
    {
        public int TenantId { get; set; }
    }

    public class CreateWalletContractBaseModel
    {
        public List<int> PlanIds { get; set; }
        public int OrganizationId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? TenantIpgSettingId { get; set; }
        public bool AssignWalletToOrganizationCustomers { get; set; }
        public List<int> Customers { get; set; }
        public WalletContractGuarantorDto Guarantor { get; set; }
        public WalletContractFinancierDto Financier { get; set; }
        public List<WalletContractFacilitatorDto> Facilitators { get; set; }
    }
}
