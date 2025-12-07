using System.Threading;
using System.Threading.Tasks;

namespace Application.Service.Contracts;

public interface IMerchantBillingService
{
    Task IssueOrOverdueBilling(CancellationToken cancellationToken);
}