namespace Application.Service.Dtos.FileManagers
{
    public class UploadFileConfiguration
    {
        public TenantMerchantContractDocuments TenantMerchantContract { get; set; }
        public PlanAttachment PlanAttachment { get; set; }
    }

    public class UploadFileConfigurationBase
    {
        public string ExpectedType { get; set; }
        public int ExpectedMaxFileLength { get; set; }
    }
    public class BusinessDocumentImage : UploadFileConfigurationBase { }
    public class NationalCartImageBack : UploadFileConfigurationBase { }
    public class NationalCartImageFront : UploadFileConfigurationBase { }
    public class OfficialNewspaper : UploadFileConfigurationBase { }
    public class LeaseAgreement : UploadFileConfigurationBase { }
    public class PropertyDeed : UploadFileConfigurationBase { }
    public class BusinessLicense : UploadFileConfigurationBase { }
    public class PlanAttachment : UploadFileConfigurationBase { }


    public class TenantMerchantContractDocuments
    {
        public NationalCartImageFront NationalCartImageFront { get; set; }
        public NationalCartImageBack NationalCartImageBack { get; set; }
        public BusinessDocumentImage BusinessDocumentImage { get; set; }
        public OfficialNewspaper OfficialNewspaper { get; set; }
        public LeaseAgreement LeaseAgreement { get; set; }
        public PropertyDeed PropertyDeed { get; set; }
        public BusinessLicense BusinessLicense { get; set; }
    }

}
