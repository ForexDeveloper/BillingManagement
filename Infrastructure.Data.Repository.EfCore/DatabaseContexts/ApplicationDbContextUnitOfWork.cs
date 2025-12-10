using System.Threading;
using System.Threading.Tasks;
using Domain.Core.UnitOfWorkContracts;

namespace Infrastructure.Data.Repository.EfCore.DatabaseContexts
{
    public sealed class ApplicationDbContextUnitOfWork(ApplicationDbContext applicationDbContext)
        : IApplicationDbContextUnitOfWork
    {
        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await applicationDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> CanConnectDatabaseAsync(CancellationToken cancellationToken)
        {
            return await applicationDbContext.Database.CanConnectAsync(cancellationToken);
        }

        public void ClearChangeTracker()
        {
            applicationDbContext.ChangeTracker.Clear();
        }
    }
}
