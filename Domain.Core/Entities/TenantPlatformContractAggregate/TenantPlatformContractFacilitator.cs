using Domain.Base;
using Domain.Core.Entities.FacilitatorAggregate;
using Domain.Core.Enums;

namespace Domain.Core.Entities.TenantPlatformContractAggregate
{
    public class TenantPlatformContractFacilitator : BaseEntity<int>
    {
        public int TenantPlatformContractId { get; private set; }
        public TenantPlatformContract TenantPlatformContract { get; private set; }
        public int FacilitatorId { get; private set; }
        public Facilitator Facilitator { get; private set; }
        public decimal? FixedAmountCommissionPercentage { get; private set; }
        public decimal? TransactionsCommissionPercentage { get; private set; }
        public BmPaymentMethodType? PaymentMethodType { get; private set; }

        public TenantPlatformContractFacilitator(int facilitatorId, decimal? fixedAmountCommissionPercentage,
        decimal? transactionsCommissionPercentage, BmPaymentMethodType? paymentMethodType)
        {
            FacilitatorId = facilitatorId;
            FixedAmountCommissionPercentage = fixedAmountCommissionPercentage;
            TransactionsCommissionPercentage = transactionsCommissionPercentage;
            PaymentMethodType = paymentMethodType;
        }

        public void Update(decimal? fixedAmountCommissionPercentage,
        decimal? transactionsCommissionPercentage, BmPaymentMethodType? paymentMethodType)
        {
            FixedAmountCommissionPercentage = fixedAmountCommissionPercentage;
            TransactionsCommissionPercentage = transactionsCommissionPercentage;
            PaymentMethodType = paymentMethodType;
        }
    }
}
