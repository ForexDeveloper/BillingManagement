using Domain.Core.Enums;
using System;
using System.Collections.Generic;

namespace Application.Query.ViewModels.WalletContracts
{
    public class GetWalletContractVm
    {
        public int Id { get; set; }
        public string ContractNumber { get; set; }
        public int TenantId { get; set; }
        public string TenantName { get; set; }
        public WalletContractStatus Status { get; set; }
        public string StatusTitle { get; set; }
        public int OrganizationId { get; set; }
        public string OrganizationTitle { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool AssignWalletToOrganizationCustomers { get; set; } = true;
        public bool IsEditable { get; set; }
        public bool CanAcceptOrReject { get; set; }
        public int? TenantIpgSettingId { get; set; }
        public string TenantIpgSettingName { get; set; }
        public WalletType WalletType { get; set; }
        public List<WalletContractPlanVm> Plans { get; set; }
        public WalletContractRejectionVm Rejection { get; set; }
        public WalletContractGuarantorVm Guarantor { get; set; }
        public WalletContractFinancierVm Financier { get; set; }
        public List<WalletContractFacilitatorVm> Facilitators { get; set; }
    }
}