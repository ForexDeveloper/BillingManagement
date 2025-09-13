using Application.Query.ReadOnlyRepositoryContracts;
using Domain.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Shared.MinIO.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.ReadonlyRepositories
{
    public class AttachmentReadOnlyRepository : IAttachmentReadOnlyRepository
    {
        private readonly ReadonlyApplicationDbContext _readonlyApplicationDbContext;

        public AttachmentReadOnlyRepository(ReadonlyApplicationDbContext readonlyApplicationDbContext)
        {
            _readonlyApplicationDbContext = readonlyApplicationDbContext;
        }

        public async Task<List<Attachment>> GetListAsync(EntityType entityType, int entityId)
        {
            var result = await _readonlyApplicationDbContext.Attachment
                .Where(x =>
                    x.EntityType == (byte)entityType &&
                    x.EntityId == entityId.ToString() &&
                    !x.IsDeleted).ToListAsync();
            return result;
        }

        public async Task<Attachment> GetByFileReference(Guid fileReference)
        {
            var result = await _readonlyApplicationDbContext.Attachment
                .FirstOrDefaultAsync(x => x.FileReference == fileReference.ToString() && !x.IsDeleted);

            return result;
        }

        public async Task<Attachment> GetAsync(EntityType entityType, int entityId)
       => await _readonlyApplicationDbContext.Attachment
                .FirstOrDefaultAsync(x => x.EntityId == entityId.ToString() && x.EntityType == (byte)entityType && !x.IsDeleted);

        public async Task<List<Attachment>> GetListAsync(EntityType entityType, List<int> entityIds)
        {
            var keys = entityIds.Select(id => id.ToString());
            return await _readonlyApplicationDbContext.Attachment
               .Where(x => keys.Contains(x.EntityId) && x.EntityType == (byte)entityType && !x.IsDeleted).ToListAsync();
        }

        public async Task<Dictionary<string, string>> GetAllFileReferencesAsync(EntityType entityType, IEnumerable<string> entityIds, CancellationToken cancellationToken)
        {
            return await _readonlyApplicationDbContext.Attachment
                .Where(x => entityIds.Contains(x.EntityId) && x.EntityType == (byte)entityType && !x.IsDeleted)
                .Select(p => new
                {
                    p.EntityId,
                    p.FileReference
                })
                .ToDictionaryAsync(p => p.EntityId, p => p.FileReference, cancellationToken);
        }
    }
}