using Application.Query.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Query.ReadOnlyRepositoryContracts;

public interface IProjectManagerReadOnlyRepository
{
   Task<List<ProjectManagerQueryModel>> GetByTenantIdAsync(int tenantId);
}
