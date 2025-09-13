using System;

namespace Application.Query.ViewModels.TenantMerchantContracts
{
    public class TenantMerchantContractsVm
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string TenantName { get; set; }
        public int MerchantId { get; set; }
        public string MerchantName { get; set; }
        public string ContractNumber { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public bool Status { get; set; }
        public bool IsEditable { get; set; }
    }
}
