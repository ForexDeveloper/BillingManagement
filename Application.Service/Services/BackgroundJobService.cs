using System.Threading;
using Domain.Core.Constants;
using System.Threading.Tasks;
using Application.Service.Contracts;
using Domain.Core.UnitOfWorkContracts;
using Domain.Core.Entities.BackgroundJobAggregate;

namespace Application.Service.Services;

public sealed class BackgroundJobService(
    IBackgroundJobRepository repository,
    IApplicationDbContextUnitOfWork unitOfWork) : IBackgroundJobService
{
    public async Task CreateMerchantBillingJobAsync(CancellationToken cancellationToken)
    {
        var jobId = BackgroundJobConstants.MerchantBilling;

        var job = await repository.GetByJobIdAsync(jobId);

        if (job == null)
        {
            job = new BackgroundJob(BackgroundJobConstants.MerchantBilling);

            await repository.AddAsync(job, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}