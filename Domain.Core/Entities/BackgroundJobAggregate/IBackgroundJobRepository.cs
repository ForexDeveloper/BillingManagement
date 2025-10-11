using System.Threading.Tasks;

namespace Domain.Core.Entities.BackgroundJobAggregate;

public interface IBackgroundJobRepository : IRepository<BackgroundJob, int>
{
    Task<BackgroundJob> GetByJobIdAsync(string jobId);
}