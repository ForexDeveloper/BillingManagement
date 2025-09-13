using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Core.Entities.ClosedloopAggregate
{
    public interface IClosedloopRepository
    {
        Task AddAsync(Closedloop cmd);
        void Update(Closedloop cmd);
        Task<Closedloop> GetByIdAsync(int id);
        Task<bool> CheckClosedloop(int walletConfigurationId ,List<int> closedloops);
    }
}