using System.Threading;
using System.Threading.Tasks;

namespace Domain.Core.UnitOfWorkContracts;

public interface IApplicationDbContextUnitOfWork
{
    void ClearChangeTracker();

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task BeginTransactionAsync(CancellationToken cancellationToken);

    Task CommitTransactionAsync(CancellationToken cancellationToken);

    Task RollbackTransactionAsync(CancellationToken cancellationToken);

    Task<bool> CanConnectDatabaseAsync(CancellationToken cancellationToken);
}