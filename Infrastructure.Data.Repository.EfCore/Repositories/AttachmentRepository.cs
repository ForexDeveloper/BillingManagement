using Domain.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Shared.MinIO.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Attachment = Shared.MinIO.Entities.Attachment;

namespace Infrastructure.Data.Repository.EfCore.Repositories
{
    public class AttachmentRepository : IAttachmentRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public AttachmentRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task AddAsync(Attachment attachment)
        {
            await _applicationDbContext.Set<Attachment>().AddAsync(attachment);
        }

        public async Task AddRangeAsync(List<Attachment> attachments)
        {
            await _applicationDbContext.Set<Attachment>().AddRangeAsync(attachments);
        }

        public void Update(Attachment attachment)
        {
            _applicationDbContext.Set<Attachment>().Update(attachment);
        }

        public void UpdateRange(List<Attachment> attachments)
        {
            _applicationDbContext.Set<Attachment>().UpdateRange(attachments);
        }

        public async Task<List<Attachment>> GetListAsync(int entityId, byte entityType)
        {
            var result = await _applicationDbContext.Set<Attachment>()
                .Where(x => x.EntityId == entityId.ToString() && x.EntityType == entityType && !x.IsDeleted).ToListAsync();

            return result;
        }
        public async Task<List<Attachment>> GetListAsync(List<string> keys, byte entityType)
        {
            var result = await _applicationDbContext.Set<Attachment>()
                .Where(x => keys.Contains(x.EntityId) && x.EntityType == entityType && !x.IsDeleted).ToListAsync();

            return result;
        }
        public async Task<Attachment> GetAttachmentAsync(int entityId, byte entityType, byte attachmentCategory)
        {
            var result = await _applicationDbContext.Set<Attachment>()
                .Where(x => x.AttachmentCategory == (byte)attachmentCategory && x.EntityType == (byte)entityType && x.EntityId == entityId.ToString() && !x.IsDeleted).FirstOrDefaultAsync();
            return result;
        }

        public async Task<Attachment> GetByIdAsync(long id)
        {
            var result = await _applicationDbContext.Set<Attachment>()
                .Where(x => x.Id == id).FirstOrDefaultAsync();

            return result;
        }
    }
}
