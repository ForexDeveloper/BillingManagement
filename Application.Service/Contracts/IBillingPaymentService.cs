using Application.Service.Dtos.BillingPayments;
using System.Threading.Tasks;

namespace Application.Service.Contracts;

public interface IBillingPaymentService
{
    Task CreateSettlemetChequePayment(SettlementChequePaymentDto settlementChequePaymentDto);
}
