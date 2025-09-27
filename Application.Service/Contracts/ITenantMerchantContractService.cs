using Application.Service.Dtos.TenantMerchantContracts;
using Domain.Core.Entities.TenantMerchantContractAggregate;
using Domain.Core.Enums;
using Shared.MinIO.Entities;
using Shared.MinIO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Service.Contracts
{
    public interface ITenantMerchantContractService
    {
        Task<List<Attachment>> SetTenantMerchantContractAttachments(int entityId, TenantMerchantContractDocumentDto documents, string userId, string clientId);
        void SetTenantMerchantContractDocument(TenantMerchantContractDocumentDto request, IdentityTypeEnum merchantType);
        Attachment FillAttachmentList(GetInfoResponse objectInfo, int entityId, AttachmentCategory attachmentCategory, string? userId, string? clientId);
        Task ValidateInputData(int tenantId, int merchantId, TenantMerchantContractDocumentDto ContractDocument);
        void PublishTenantMerchantContractAddedOrUpdatedEvent(TenantMerchantContract contract);
    }
}
