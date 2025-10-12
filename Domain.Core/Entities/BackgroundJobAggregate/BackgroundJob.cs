using Domain.Base;

namespace Domain.Core.Entities.BackgroundJobAggregate;

public sealed class BackgroundJob : BaseEntity<int>
{
    public string JobId { get; set; }

    private BackgroundJob()
    {
        
    }

    public BackgroundJob(string jobId)
    {
        JobId = jobId;
    }
}