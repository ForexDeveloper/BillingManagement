using Application.Service.Dtos.WalletContract;

namespace Service.Rest.V1.RequestModels.TenantMerchantContracts;


public class CloneWalletContractModel : CloneWalletContractBaseModel
{
    public int TenantId { get; set; }
}

public class CloneWalletContractBaseModel
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? TenantIpgSettingId { get; set; }
    public bool AssignWalletToOrganizationCustomers { get; set; }
    public List<int> Customers { get; set; }
    public WalletContractGuarantorDto Guarantor { get; set; }
    public WalletContractFinancierDto Financier { get; set; }
    public List<WalletContractFacilitatorDto> Facilitators { get; set; }
}