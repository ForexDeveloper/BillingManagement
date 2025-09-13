using Domain.Core.Enums;

namespace Application.Query.ViewModels.TenantMerchantContracts;

public class GetTenantMerchantDocumentVm
{
    public string FileReference { get; set; }
    public long FileSize { get; set; }
    public string FileExtension { get; set; }
    public string ContentType { get; set; }
    public AttachmentCategory AttachmentCategory { get; set; }
    public string AttachmentCategoryName { get; set; }
}