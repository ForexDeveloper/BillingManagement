using System.Threading.Tasks;

namespace Domain.Core.Entities.FinancierAggregate
{
    public interface IFinancierRepository
    {
        Task AddAsync(Financier financier);
        void Update(Financier financier);
        Task<Financier> GetAsync(int id);
        Task<bool> IsFinancierBelongToTenantAsync(int tenantId, int financierId);
        Task<Financier> GetByTenantIdAsync(int tenantId);
    }
}