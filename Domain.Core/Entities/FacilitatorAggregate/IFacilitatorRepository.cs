using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Core.Entities.FacilitatorAggregate
{
    public interface IFacilitatorRepository
    {
        Task AddAsync(Facilitator facilitator);
        void Update(Facilitator facilitator);
        Task<Facilitator> GetAsync(int id);
        Task<bool> FacilitatorsBelongToTenantAsync(List<int> facilitatorsId, int tenantId);
        Task<Facilitator> GetByTenantIdAsync(int tenantId);
    }
}