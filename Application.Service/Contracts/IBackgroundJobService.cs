using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Service.Contracts;

public interface IBackgroundJobService
{
    Task<DateTime> CreateMerchantBillingJobAsync(CancellationToken cancellationToken);
}