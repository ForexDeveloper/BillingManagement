using Shared.MinIO.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.MinIO.Entities
{

    public class Attachment
    {
        public long Id { get; private set; }
        public EntityType EntityType { get; private set; }
        public string FileReference { get; private set; }
        public string EntityId { get; private set; }
        public long FileSize { get; private set; }
        public string FileExtension { get; private set; }
        public string ContentType { get; private set; }
        public AttachmentCategory AttachmentCategory { get; private set; }

        private Attachment()
        {

        }
        public Attachment(EntityType entityType, string fileReference, string entityId, long fileSize, string fileExtension, string contentType, AttachmentCategory attachmentCategory, string? userId = null, string? clientId = null)
        {
            EntityType = entityType;
            FileReference = fileReference;
            EntityId = entityId;
            FileSize = fileSize;
            FileExtension = fileExtension;
            ContentType = contentType;
            AttachmentCategory = attachmentCategory;
            SetCreatorUserId(userId);
            SetClientId(clientId);
        }


        public DateTime CreatedDateTime { get; protected set; } = DateTime.Now;
        public DateTime EditDateTime { get; protected set; } = DateTime.Now;


        [MaxLength(100)]
        [Column(TypeName = "VARCHAR")]
        public string? CreatorUserId { get; protected set; }

        [MaxLength(100)]
        [Column(TypeName = "VARCHAR")]
        public string? ClientId { get; protected set; }

        public bool IsDeleted { get; protected set; }

        public void SetCreatorUserId(string? userId)
        {
            CreatorUserId = userId;
        }

        public void SetClientId(string? clientId)
        {
            ClientId = clientId;
        }

        public void SetEditDateTime(DateTime editDateTime)
        {
            EditDateTime = editDateTime;
        }
        public void SetDeleted()
        {
            IsDeleted = true;
        }

    }
}
