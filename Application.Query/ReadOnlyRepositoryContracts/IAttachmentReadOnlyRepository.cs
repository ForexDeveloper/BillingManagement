using Domain.Core.Enums;
using Shared.MinIO.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.ReadOnlyRepositoryContracts
{
    public interface IAttachmentReadOnlyRepository
    {
        Task<List<Attachment>> GetListAsync(EntityType entityType, int entityId);

        Task<Attachment> GetAsync(EntityType entityType, int entityId);

        Task<Attachment> GetByFileReference(Guid fileReference);

        Task<List<Attachment>> GetListAsync(EntityType entityType, List<int> entityIds);

        Task<Dictionary<string, string>> GetAllFileReferencesAsync(EntityType entityType, IEnumerable<string> entityIds,
            CancellationToken cancellationToken);
    }
}
