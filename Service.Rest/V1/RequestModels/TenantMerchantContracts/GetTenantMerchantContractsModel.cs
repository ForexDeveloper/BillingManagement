using Application.Query.Base;
using Domain.Core.Enums;

namespace Service.Rest.V1.RequestModels.TenantMerchantContracts
{
    public class GetTenantMerchantContractsModel : GetTenantMerchantContractsBaseModel
    {
        public int? TenantId { get; set; }
    }

    public class GetTenantMerchantContractsBaseModel : BasePaginatedListRequest
    {
        public int? MerchantId { get; set; }
        public GuaranteeType? GuaranteeType { get; set; }
        public SettlementType? SettlementType { get; set; }
        public PaymentMethodType? PaymentMethodType { get; set; }
    }
}
