using Application.Service.Dtos.CashOut;
using Application.Service.Dtos.CashWallet;
using Domain.Core.Entities.CashOutRequestAggregate;
using Domain.Core.Entities.CustomerAggregate;
using Domain.Core.Enums;
using Shared.EventBus.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Service.Contracts;

public interface ICashOutRequestService
{
    Task<CashOutResult> CreateCashOutRequest(CreateCashOutRequestDto request, CancellationToken cancellationToken);

    Task AproveCustomerCashOutRequest(CashOutRequest cashOutRequest, string bankTransactionCode, string description, Customer customer, CancellationToken cancellationToken);

    Task RejectCustomerCashOutRequest(CashOutRequest cashOutRequest, CashOutRequestRejectReason reason, string description, Customer customer, CancellationToken cancellationToken);

    Task<Customer> ValidateCustomer(int customerId, int tenantId);

    Task ValidateBankAccount(int bankAccountId, Customer customer);

    Task<CashOutRequest> GetCashOutRequest(long cashOutRequestId, int customerId, int tenantId);

    void SendApprovedOrRejectedSms(string fullName, string mobile, string userId, int tenantId, NotificationTemplateActionType notificationTemplateActionType, long? followUpCode = null);
}