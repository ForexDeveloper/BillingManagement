using Domain.Core.Enums;
using System;

namespace Application.Service.Dtos.TenantMerchantContracts
{
    public class TenantMerchantContractDocumentDto
    {
        public Guid? NationalCartImageFront { get; set; }
        public Guid? NationalCartImageBack { get; set; }
        public BusinessDocumentType? BusinessDocumentType { get; set; }
        public Guid? BusinessDocumentImage { get; set; }
        public Guid? OfficialNewspaper { get; set; }
        public string EnamadLink { get; set; }
        public string InternetBusinessLicenseLink { get; set; }
    }
}
