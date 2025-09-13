using Domain.Base;
using Domain.Core.AggregateRoots.WalletConfigurationAggregate;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Core.Entities.ProjectManegerAggregate
{
    public interface IProjectManagerRepository
    {
        Task AddAsync(ProjectManager tenant);
        void Update(ProjectManager tenant);
        Task<ProjectManager> GetAsync(int tenantId, string userId);
        Task<bool> CheckProjectManagers(int id);

    }
}