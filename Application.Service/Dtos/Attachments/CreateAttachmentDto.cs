
using Domain.Core.Enums;

namespace Application.Service.Dtos.Attachments
{
    public class AttachmentDto
    {
        public string FileReference { get; set; }
        public int EntityId { get; set; }
        public AttachmentCategory AttachmentCategory { get; set; }
        public EntityType EntityType { get; set; }

    }
}
