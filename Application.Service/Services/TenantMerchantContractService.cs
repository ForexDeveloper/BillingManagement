using Application.Service.Contracts;
using Application.Service.Dtos.TenantMerchantContracts;
using Domain.Core.Entities.Document;
using Domain.Core.Entities.MerchantAggregate;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate;
using Domain.Core.Enums;
using Domain.Core.Helper;
using Shared.EventBus.Contracts;
using Shared.EventBus.Events;
using Shared.MinIO.Contracts;
using Shared.MinIO.Entities;
using Shared.MinIO.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Service.Services
{
    public class TenantMerchantContractService : ITenantMerchantContractService
    {
        private readonly IFileManagerService _fileManagerService;
        private readonly ITenantRepository _tenantRepository;
        private readonly IMerchantRepository _merchantRepository;
        private readonly IOutboxService _outboxService;

        public TenantMerchantContractService(IFileManagerService fileManagerService,
            ITenantRepository tenantRepository,
            IMerchantRepository merchantRepository,
            IOutboxService outboxService)
        {
            _fileManagerService = fileManagerService;
            _tenantRepository = tenantRepository;
            _merchantRepository = merchantRepository;
            _outboxService = outboxService;
        }

        public async Task<List<Attachment>> SetTenantMerchantContractAttachments(int entityId, TenantMerchantContractDocumentDto documents, string userId,
            string clientId)
        {
            var businessDocumentType = documents.BusinessDocumentType;
            var nationalCartImageFront = documents.NationalCartImageFront.ToString();
            var nationalCartImageBack = documents.NationalCartImageBack.ToString();
            var businessDocumentImage = documents.BusinessDocumentImage.ToString();
            var officialNewspaper = documents.OfficialNewspaper.ToString();

            var attachments = new List<Attachment>();

            var documentMappings = new Dictionary<string, AttachmentCategory>();

            if (!string.IsNullOrEmpty(nationalCartImageFront))
            {
                documentMappings.Add(nationalCartImageFront, AttachmentCategory.CartMeliFront);
            }

            if (!string.IsNullOrEmpty(nationalCartImageBack))
            {
                documentMappings.Add(nationalCartImageBack, AttachmentCategory.CartMeliBack);
            }

            if (!string.IsNullOrEmpty(businessDocumentImage) && businessDocumentType != null)
            {
                switch (businessDocumentType)
                {
                    case BusinessDocumentType.BusinessLicense:
                        documentMappings.Add(businessDocumentImage, AttachmentCategory.BusinessLicense);
                        break;
                    case BusinessDocumentType.PropertyDeed:
                        documentMappings.Add(businessDocumentImage, AttachmentCategory.PropertyDeed);
                        break;
                    case BusinessDocumentType.LeaseAgreement:
                        documentMappings.Add(businessDocumentImage, AttachmentCategory.LeaseAgreement);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(businessDocumentType), businessDocumentType, $"مقدار {businessDocumentType} صحیح نیست.");
                }
            }

            if (!string.IsNullOrEmpty(officialNewspaper))
            {
                documentMappings.Add(officialNewspaper, AttachmentCategory.OfficialNewspaper);
            }

            foreach (var mapping in documentMappings)
            {
                var objectInfo = await _fileManagerService.GetObjectInfo(mapping.Key, EntityType.TenantMerchantContract.ToString());
                if (!objectInfo.Result)
                {
                    throw new DocumentNotFoundException($"فایل {mapping.Value.GetEnumDescription()} پیدا نشد.");
                }
                attachments.Add(FillAttachmentList(objectInfo, entityId, documentMappings[mapping.Key], userId, clientId));
            }

            return attachments;
        }

        public void SetTenantMerchantContractDocument(TenantMerchantContractDocumentDto request, IdentityTypeEnum merchantType)
        {
            if (merchantType == IdentityTypeEnum.Individual)
            {
                request.OfficialNewspaper = null;
            }
            else
            {
                request.NationalCartImageFront = null;
                request.NationalCartImageBack = null;
                request.BusinessDocumentType = null;
                request.BusinessDocumentImage = null;
            }
        }

        public Attachment FillAttachmentList(GetInfoResponse objectInfo, int entityId, AttachmentCategory attachmentCategory, string? userId, string? clientId)
        {
            if (string.IsNullOrEmpty(objectInfo.FileReference) || string.IsNullOrEmpty(objectInfo.FileExtension)
                                                               || string.IsNullOrEmpty(objectInfo.ContentType) || objectInfo.FileSize == null)
            {
                throw new DocumentInvalidDataException("متا داده های فایل به درستی ذخیره نشده اند.");
            }

            return new Attachment(
                (byte)EntityType.TenantMerchantContract,
                objectInfo.FileReference,
                entityId.ToString(),
                objectInfo.FileSize.Value,
                objectInfo.FileExtension,
                objectInfo.ContentType,
                (byte)attachmentCategory,
                userId,
                clientId
            );
        }

        public async Task ValidateInputData(int tenantId, int merchantId, TenantMerchantContractDocumentDto ContractDocument)
        {
            var tenant = await _tenantRepository.GetAsync(tenantId);
            if (tenant is null)
                throw new ArgumentValidationException(nameof(tenantId), "مالک زیر ساخت پیدا نشد.");

            var merchant = await _merchantRepository.GetAsync(merchantId, tenantId);
            if (merchant is null)
                throw new ArgumentValidationException(nameof(merchantId), "پذیرنده پیدا نشد.");

            if (merchant.Type == IdentityTypeEnum.Individual)
            {
                if (!ContractDocument.NationalCartImageFront.HasValue)
                    throw new ArgumentValidationException(nameof(ContractDocument.NationalCartImageFront), "تصویر روی کارت ملی اجباریست.");

                if (!ContractDocument.NationalCartImageBack.HasValue)
                    throw new ArgumentValidationException(nameof(ContractDocument.NationalCartImageBack), "تصویر پشت کارت ملی اجباریست.");

                if (ContractDocument.BusinessDocumentType == null)
                    throw new ArgumentValidationException(nameof(ContractDocument.BusinessDocumentType), "انتخاب نوع مدرک اجباریست.");

                if (!ContractDocument.BusinessDocumentImage.HasValue)
                    throw new ArgumentValidationException(nameof(ContractDocument.BusinessDocumentImage), "انتخاب تصویر جواز کسب یا سند یا اجاره‌نامه اجباریست.");
            }

            if (merchant.Type == IdentityTypeEnum.Legal)
            {
                if (!ContractDocument.OfficialNewspaper.HasValue)
                    throw new ArgumentValidationException(nameof(ContractDocument.OfficialNewspaper), "تصویر آخرین روزنامه رسمی اجباریست.");
            }

        }

        public void PublishTenantMerchantContractAddedOrUpdatedEvent(TenantMerchantContract contract)
        {
            _outboxService.AddNewEvent(new BmTenantMerchantContractAddedOrUpdatedEvent()
            {
                Id = contract.Id,
                TenantId = contract.TenantId,
                MerchantId = contract.MerchantId,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                Status = contract.Status
            });
        }
    }
}
