using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Domain.Core.Entities.BackgroundJobAggregate;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;

namespace Infrastructure.Data.Repository.EfCore.Repositories;

public sealed class BackgroundJobRepository(ApplicationDbContext applicationDbContext)
    : Repository<BackgroundJob, int>(applicationDbContext), IBackgroundJobRepository
{
    private readonly ApplicationDbContext _applicationDbContext = applicationDbContext;

    public async Task<BackgroundJob> GetByJobIdAsync(string jobId)
    {
        return await _applicationDbContext.BackgroundJobs.FirstOrDefaultAsync(p => p.JobId == jobId);
    }
}