using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Application.Service.Contracts;

namespace Application.Command.MerchantBillingCommands;

public sealed class IssueOrOverdueMerchantBillingsCommand : IRequest;

public sealed class IssueOrOverdueMerchantBillingsCommandHandler(IMerchantBillingService service)
    : IRequestHandler<IssueOrOverdueMerchantBillingsCommand>
{
    public async Task Handle(IssueOrOverdueMerchantBillingsCommand command, CancellationToken cancellationToken)
    {
        await service.IssueOrOverdueBillings(cancellationToken);
    }
}