using System.Threading.Tasks;

namespace Domain.Core.Entities.GuarantorAggregate
{
    public interface IGuarantorRepository
    {
        Task AddAsync(Guarantor guarantor);
        void Update(Guarantor guarantor);
        Task<Guarantor> GetAsync(int id);
        Task<bool> IsGuarantorBelongToTenantAsync(int tenantId, int guarantorId);
        Task<Guarantor> GetByTenantIdAsync(int tenantId);
    }
}