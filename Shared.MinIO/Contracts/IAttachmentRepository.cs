using Shared.MinIO.Entities;
using Shared.MinIO.Enums;

namespace Shared.MinIO.Contracts
{
    public interface IAttachmentRepository
    {
        Task AddAsync(Attachment attachment);
        Task AddRangeAsync(List<Attachment> attachments);
        void Update(Attachment attachment);
        void UpdateRange(List<Attachment> attachments);
        Task<List<Attachment>> GetListAsync(EntityType entityType, int entityId);
        Task<bool> CheckAttachmentAsync(int entityId, EntityType entityType, string fileReference);
        Task<List<Attachment>> GetListAsync(List<string> keys);
    }
}
