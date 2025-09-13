using Application.Service.Dtos.Wallet;
using Shared.EventBus.Events;
using System.Threading.Tasks;

namespace Application.Service.Contracts;

public interface ILoanWalletService
{
    Task<OperationResult> CreateCustomerWalletByGranting(CgmProcessInstanceAddedEvent context);
    Task<OperationResult> CreateCustomerChequeWalletByGranting(CgmSettlementChequesProcessInstanceAddedEvent context);
}
