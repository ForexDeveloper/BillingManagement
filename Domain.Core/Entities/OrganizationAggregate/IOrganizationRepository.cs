using System.Threading.Tasks;

namespace Domain.Core.Entities.OrganizationAggregate
{
    public interface IOrganizationRepository
    {
        Task AddAsync(Organization organization);
        void Update(Organization organization);
        Task<Organization> GetAsync(int id);
        Task<Organization> GetByTenantIdAsync(int tenantId);
    }
}