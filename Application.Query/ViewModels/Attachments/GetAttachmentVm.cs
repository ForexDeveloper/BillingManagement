using Domain.Core.Enums;
using Domain.Core.Helper;

namespace Application.Query.ViewModels.Attachments
{
    public class GetAttachmentVm
    {
        public string FileReference { get; set; }
        public long FileSize { get; set; }
        public string FileExtension { get; set; }
        public string ContentType { get; set; }
        public AttachmentCategory AttachmentCategory { get; set; }
        public string AttachmentCategoryName => AttachmentCategory.GetEnumDescription();
    }
}
