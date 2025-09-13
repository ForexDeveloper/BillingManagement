using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.TenantMerchantContracts;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Enums;
using Domain.Core.Helper;
using MediatR;
using Shared.MinIO.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Core.Entities.TenantMerchantContractAggregate;

namespace Application.Query.Queries
{
    public class GetTenantMerchantContractAttachmentsByContactIdQuery : IRequest<List<GetTenantMerchantDocumentVm>>
    {
        public int Id { get; }
        public int? TenantId { get; }
        public GetTenantMerchantContractAttachmentsByContactIdQuery(int id, int? tenantId = null)
        {
            Id = id;
            TenantId = tenantId;
        }
    }

    public class GetTenantMerchantContractAttachmentsByContactIdQueryHandler : IRequestHandler<GetTenantMerchantContractAttachmentsByContactIdQuery, List<GetTenantMerchantDocumentVm>>
    {
        private readonly IAttachmentReadOnlyRepository _attachmentRepositoryReadOnlyRepository;
        private readonly ITenantMerchantContractRepository _tenantMerchantContractRepository;

        public GetTenantMerchantContractAttachmentsByContactIdQueryHandler(IAttachmentReadOnlyRepository attachmentRepositoryReadOnlyRepository, ITenantMerchantContractRepository tenantMerchantContractRepository)
        {
            _attachmentRepositoryReadOnlyRepository = attachmentRepositoryReadOnlyRepository;
            _tenantMerchantContractRepository = tenantMerchantContractRepository;
        }

        public async Task<List<GetTenantMerchantDocumentVm>> Handle(GetTenantMerchantContractAttachmentsByContactIdQuery request, CancellationToken cancellationToken)
        {
            var attachments = new List<GetTenantMerchantDocumentVm>();

            if (request.TenantId.HasValue)
            {
                if (!await _tenantMerchantContractRepository.IsContractBelongToTenantAsync(request.Id, request.TenantId.Value))
                {
                    throw new ArgumentValidationException(nameof(request.Id), "قرارداد به مالک زیر ساخت تعلق ندارد.");
                }
            }

            var contractAttachments = await _attachmentRepositoryReadOnlyRepository.GetListAsync(EntityType.TenantMerchantContract, request.Id);

            if (!contractAttachments.Any()) return attachments;

            var attachmentCategories = new[]
            {
                AttachmentCategory.OfficialNewspaper,
                AttachmentCategory.CartMeliFront,
                AttachmentCategory.CartMeliBack,
                AttachmentCategory.BusinessLicense,
                AttachmentCategory.PropertyDeed,
                AttachmentCategory.LeaseAgreement
            };

            foreach (var category in attachmentCategories)
            {
                var document = CreateDocumentFromAttachment(contractAttachments, category);
                if (document != null)
                {
                    attachments.Add(document);
                }
            }

            return attachments;

        }

        private GetTenantMerchantDocumentVm CreateDocumentFromAttachment(IEnumerable<Attachment> contractAttachments, AttachmentCategory category)
        {
            var contractAttachment = contractAttachments.FirstOrDefault(x => x.AttachmentCategory == (byte)category);
            if (contractAttachment == null) return null;

            return new GetTenantMerchantDocumentVm
            {
                FileReference = contractAttachment.FileReference,
                FileExtension = contractAttachment.FileExtension,
                FileSize = contractAttachment.FileSize,
                ContentType = contractAttachment.ContentType,
                AttachmentCategory = category,
                AttachmentCategoryName = category.GetEnumDescription()
            };
        }
    }
}
