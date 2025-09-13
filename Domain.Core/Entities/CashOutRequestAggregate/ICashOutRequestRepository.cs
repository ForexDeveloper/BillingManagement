using System.Threading.Tasks;

namespace Domain.Core.Entities.CashOutRequestAggregate;

public interface ICashOutRequestRepository
{
    Task AddAsync(CashOutRequest cashOutRequest);
    Task<CashOutRequest> GetByIdAsync(long id);
}