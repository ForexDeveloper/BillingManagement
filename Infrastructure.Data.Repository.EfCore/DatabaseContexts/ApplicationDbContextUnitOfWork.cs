using System.Threading;
using System.Threading.Tasks;
using Domain.Core.UnitOfWorkContracts;

namespace Infrastructure.Data.Repository.EfCore.DatabaseContexts;

public sealed class ApplicationDbContextUnitOfWork(ApplicationDbContext applicationDbContext)
    : IApplicationDbContextUnitOfWork
{
    public void ClearChangeTracker()
    {
        applicationDbContext.ChangeTracker.Clear();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await applicationDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken)
    {
        await applicationDbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken)
    {
        await applicationDbContext.Database.CommitTransactionAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken)
    {
        await applicationDbContext.Database.RollbackTransactionAsync(cancellationToken);
    }

    public async Task<bool> CanConnectDatabaseAsync(CancellationToken cancellationToken)
    {
        return await applicationDbContext.Database.CanConnectAsync(cancellationToken);
    }
}