using Domain.Base;
using System.Threading;
using Domain.Core.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;

namespace Infrastructure.Data.Repository.EfCore.Repositories;

public abstract class Repository<TEntity, TKey>(ApplicationDbContext applicationDbContext)
    : IRepository<TEntity, TKey> where TEntity : BaseEntity<TKey>
{
    protected readonly DbSet<TEntity> Entities = applicationDbContext.Set<TEntity>();

    public virtual async Task AddAsync(TEntity entity, CancellationToken? cancellationToken = null)
    {
        await Entities.AddAsync(entity, cancellationToken ?? CancellationToken.None);
    }

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken? cancellationToken = null)
    {
        await Entities.AddRangeAsync(entities, cancellationToken ?? CancellationToken.None);
    }

    public virtual void UpdatePartial(TEntity entity, string property)
    {
        Entities.Attach(entity);
        Entities.Entry(entity).Property(property).IsModified = true;
    }

    public virtual void UpdatePartial(TEntity entity, IEnumerable<string> properties)
    {
        Entities.Attach(entity);

        foreach (var property in properties)
        {
            Entities.Entry(entity).Property(property).IsModified = true;
        }
    }

    public virtual async Task<TEntity> GetAsync(TKey id, CancellationToken? cancellationToken = null)
    {
        return await Entities.FirstOrDefaultAsync(p => p.Id.Equals(id), cancellationToken ?? CancellationToken.None);
    }

    public virtual async Task<bool> AnyAsync(TKey id, CancellationToken? cancellationToken = null)
    {
        return await Entities.AnyAsync(p => p.Id.Equals(id), cancellationToken ?? CancellationToken.None);
    }
}