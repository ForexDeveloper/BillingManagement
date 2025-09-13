using System.Linq;
using Domain.Core.Entities.ProjectManegerAggregate;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.Repositories
{
    public class ProjectManagerRepository : IProjectManagerRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public ProjectManagerRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task AddAsync(ProjectManager ProjectManagerr)
        {
           await _applicationDbContext.ProjectManagers.AddAsync(ProjectManagerr);
        }

      
        public async Task<ProjectManager> GetAsync(int tenantId ,string userId)
        {
            return await _applicationDbContext.ProjectManagers.FirstOrDefaultAsync(p=>p.TenantId == tenantId && p.UserId== userId);
        }

        public void Update(ProjectManager ProjectManagerr)
        {
            _applicationDbContext.ProjectManagers.Update(ProjectManagerr);
        }

        public async Task<bool> CheckProjectManagers(int id)
        {
            return await _applicationDbContext.ProjectManagers
                .Where(pm=>pm.IsActive)
                .AnyAsync(p => p.Id == id);
        }
      

    }

}
