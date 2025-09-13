using Domain.Core.Enums;

namespace Application.Query.ViewModels.TenantMerchantContracts;

public class GetTenantMerchantDocumentNameVm
{
    public string NationalCartImageFront { get; set; }
    public string NationalCartImageFrontContentType { get; set; }
    public string NationalCartImageBack { get; set; }
    public string NationalCartImageBackContentType { get; set; }
    public string BusinessDocumentImage { get; set; }
    public string BusinessDocumentImageContentType { get; set; }
    public BusinessDocumentType? BusinessDocumentType { get; set; }
    public string BusinessDocumentTypeName { get; set; }
    public string OfficialNewspaper { get; set; }
    public string OfficialNewspaperContentType { get; set; }

    public GetTenantMerchantDocumentNameVm(BusinessDocumentType? businessDocumentType = null, string businessDocumentTypeName = null,
        string nationalCartImageFront = null, string nationalCartImageFrontContentType = null,
        string nationalCartImageBack = null, string nationalCartImageBackContentType = null,
        string businessDocumentImage = null, string businessDocumentImageContentType = null,
        string officialNewspaper = null, string officialNewspaperContentType = null
        )
    {
        BusinessDocumentType = businessDocumentType;
        BusinessDocumentTypeName = businessDocumentTypeName;
        NationalCartImageFront = nationalCartImageFront;
        NationalCartImageFrontContentType = nationalCartImageFrontContentType;
        NationalCartImageBack = nationalCartImageBack;
        NationalCartImageBackContentType = nationalCartImageBackContentType;
        BusinessDocumentImage = businessDocumentImage;
        BusinessDocumentImageContentType = businessDocumentImageContentType;
        OfficialNewspaper = officialNewspaper;
        OfficialNewspaperContentType = officialNewspaperContentType;
    }
}