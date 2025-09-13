using Domain.Core.Enums;

namespace Service.Rest.V1.RequestModels.FileManagers
{
    public class UploadDocumentModel
    {
        public IFormFile File { get; set; }
        public EntityType FolderName { get; set; }
        public AttachmentCategory AttachmentCategory { get; set; }
    }
}
