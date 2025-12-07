using System.Threading;
using System.Threading.Tasks;

namespace Application.Service.Contracts;

public interface IBackgroundJobService
{
    Task CreateMerchantBillingJobAsync(CancellationToken cancellationToken);
}