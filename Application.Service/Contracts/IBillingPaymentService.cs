using Application.Service.Dtos.MerchantBillings;
using Shared.EventBus.Events;
using System.Threading.Tasks;

namespace Application.Service.Contracts;
public interface IBillingPaymentService
{
    bool IsMerchantBillingPayable(MerchantBillingPayableDto request);
    Task MerchantBillingPayment(PmBillingManualPaymentUpdateStateEvent requset);
}
