using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.Repositories
{
    public class ProjectManagerReadOnlyRepository : IProjectManagerReadOnlyRepository
    {
        private readonly ReadonlyApplicationDbContext _readonlyApplicationDbContext;

        public ProjectManagerReadOnlyRepository(ReadonlyApplicationDbContext readonlyApplicationDbContext)
        {
            _readonlyApplicationDbContext = readonlyApplicationDbContext;
        }

        public async Task<List<ProjectManagerQueryModel>> GetByTenantIdAsync(int tenantId)
         => await _readonlyApplicationDbContext.ProjectManagers.Where(c => c.TenantId == tenantId && c.IsActive).Select(c => new ProjectManagerQueryModel
         {
             Id = c.Id,
             FullName = c.FullName,
         }).ToListAsync();
    }

}
