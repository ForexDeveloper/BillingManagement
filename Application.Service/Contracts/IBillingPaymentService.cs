using Shared.EventBus.Events;
using System.Threading.Tasks;
using Application.Service.Dtos.MerchantBillings;

namespace Application.Service.Contracts;
public interface IBillingPaymentService
{
    bool IsMerchantBillingPayable(MerchantBillingPayableDto request);

    Task SetMerchantBillingPayment(PmBillingManualPaymentUpdateStateEvent request);
}