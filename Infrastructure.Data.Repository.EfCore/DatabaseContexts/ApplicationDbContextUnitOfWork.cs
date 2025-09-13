using Domain.Core.UnitOfWorkContracts;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.DatabaseContexts
{
    public class ApplicationDbContextUnitOfWork : IApplicationDbContextUnitOfWork
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public ApplicationDbContextUnitOfWork(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _applicationDbContext.SaveChangesAsync();
        }

        public async Task<bool> CanConnectDatabaseAsync(CancellationToken cancellationToken)
        {
            return await _applicationDbContext.Database.CanConnectAsync(cancellationToken);
        }

        public void ClearChangeTracker()
        {
            _applicationDbContext.ChangeTracker.Clear();
        }

    }
}
