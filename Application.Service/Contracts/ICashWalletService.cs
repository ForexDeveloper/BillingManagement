using System.Threading.Tasks;
using Application.Service.Dtos.BillingPayments;
using Application.Service.Dtos.CashWallet;

namespace Application.Service.Contracts;

public interface ICashWalletService
{
    Task CreateCustomerCashWalletByAddCustomerEvent(CreateCashWalletWithAddCustomerEventDto dto);
}
