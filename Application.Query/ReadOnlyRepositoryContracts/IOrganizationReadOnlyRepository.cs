using Application.Query.Queries;
using Application.Query.QueryModels;
using Application.Query.ViewModels.Organizations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Query.ReadOnlyRepositoryContracts;

public interface IOrganizationReadOnlyRepository
{
    Task<OrganizationQueryModel> GetByIdAsync(int id, int? tenantId);
    Task<List<OrganizationsVm>> GetByTenantIdAsync(int tenantId);
    Task<GetOrganizationsQueryModel> GetOrganizationsAsync(GetOrganizationsQuery request);
}
