using System.Threading;
using System.Threading.Tasks;

namespace Domain.Core.UnitOfWorkContracts
{
    public interface IApplicationDbContextUnitOfWork
    {
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<bool> CanConnectDatabaseAsync(CancellationToken cancellationToken);
        void ClearChangeTracker();
    }
}
