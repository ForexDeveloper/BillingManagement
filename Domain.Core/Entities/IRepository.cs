using Domain.Base;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Domain.Core.Entities;

public interface IRepository<TEntity, in TKey> where TEntity : BaseEntity<TKey>
{
    Task AddAsync(TEntity entity, CancellationToken? cancellationToken = null);

    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken? cancellationToken = null);

    void UpdatePartial(TEntity entity, string property);

    void UpdatePartial(TEntity entity, IEnumerable<string> properties);

    Task<TEntity> GetAsync(TKey id, CancellationToken? cancellationToken = null);
}