
using Shared.MinIO.Enums;

namespace Shared.MinIO.ViewModels;

public class AttachmentViewModel
{
    public string FileReference { get; set; }
    public long FileSize { get; set; }
    public string FileExtension { get; set; }
    public string ContentType { get; set; }
    public AttachmentCategory AttachmentCategory { get; set; }
    public string AttachmentCategoryTitle { get; set; }
}
