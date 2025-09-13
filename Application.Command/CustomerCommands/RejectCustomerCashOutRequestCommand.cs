using Application.Service.Contracts;
using Domain.Core.Enums;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Command.CustomerCommands;

public class RejectCustomerCashOutRequestCommand : IRequest<long>
{
    public int CustomerId { get; set; }
    public long CashOutRequestId { get; set; }
    public string Description { get; set; }
    public CashOutRequestRejectReason RejectReason { get; set; }
    public int TenantId { get; set; }

    public RejectCustomerCashOutRequestCommand(int customerId, long cashOutRequestId, string description,
        CashOutRequestRejectReason rejectReason,
        int tenantId)
    {
        CustomerId = customerId;
        CashOutRequestId = cashOutRequestId;
        Description = description;
        RejectReason = rejectReason;
        TenantId = tenantId;
    }
}
public class RejectCustomerCashOutCommandHandler : IRequestHandler<RejectCustomerCashOutRequestCommand, long>
{
    private readonly ICashOutRequestService _cashOutRequestService;

    public RejectCustomerCashOutCommandHandler(ICashOutRequestService cashOutRequestService)
    {
        _cashOutRequestService = cashOutRequestService;
    }

    public async Task<long> Handle(RejectCustomerCashOutRequestCommand request, CancellationToken cancellationToken)
    {
        var cashOutRequest = await _cashOutRequestService.GetCashOutRequest(request.CashOutRequestId, request.CustomerId, request.TenantId);

        var customer = await _cashOutRequestService.ValidateCustomer(request.CustomerId, cashOutRequest.TenantId);

        await _cashOutRequestService.RejectCustomerCashOutRequest(cashOutRequest, request.RejectReason, request.Description, customer, cancellationToken);

        return request.CashOutRequestId;
    }
}