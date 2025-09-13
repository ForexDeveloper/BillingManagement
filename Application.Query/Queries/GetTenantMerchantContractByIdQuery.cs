using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.TenantMerchantContracts;
using Application.Service.Contracts;
using Application.Service.Dtos.Shared;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate.Exceptions;
using Domain.Core.Enums;
using Domain.Core.Helper;
using MediatR;
using Shared.MinIO.Contracts;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetTenantMerchantContractByIdQuery : IRequest<GetTenantMerchantContractVm>
    {
        public int Id { get; }
        public int? TenantId { get; }
        public GetTenantMerchantContractByIdQuery(int id, int? tenantId = null)
        {
            Id = id;
            TenantId = tenantId;
        }
    }

    public class GetTenantMerchantContractByIdQueryHandler : IRequestHandler<GetTenantMerchantContractByIdQuery, GetTenantMerchantContractVm>
    {
        private readonly ITenantMerchantContractReadOnlyRepository _tenantMerchantContractReadOnlyRepository;
        private readonly ITenantMerchantContractRepository _tenantMerchantContractRepository;
        private readonly IAttachmentReadOnlyRepository _attachmentRepositoryReadOnlyRepository;
        private readonly IFileManagerService _fileManagerService;
        private readonly IAttachmentService _attachmentService;
        private readonly IFinancialDocumentRepository _financialDocumentRepository;

        public GetTenantMerchantContractByIdQueryHandler(ITenantMerchantContractReadOnlyRepository tenantMerchantContractReadOnlyRepository,
            IAttachmentReadOnlyRepository attachmentRepositoryReadOnlyRepository,
            IFileManagerService fileManagerService,
            IAttachmentService attachmentService, IFinancialDocumentRepository financialDocumentRepository,
            ITenantMerchantContractRepository tenantMerchantContractRepository)
        {
            _tenantMerchantContractReadOnlyRepository = tenantMerchantContractReadOnlyRepository;
            _attachmentRepositoryReadOnlyRepository = attachmentRepositoryReadOnlyRepository;
            _fileManagerService = fileManagerService;
            _attachmentService = attachmentService;
            _financialDocumentRepository = financialDocumentRepository;
            _tenantMerchantContractRepository = tenantMerchantContractRepository;
        }

        public async Task<GetTenantMerchantContractVm> Handle(GetTenantMerchantContractByIdQuery request, CancellationToken cancellationToken)
        {
            var contract = await _tenantMerchantContractReadOnlyRepository.GetByIdAsync(request.Id, request.TenantId) ?? throw new TenantMerchantContractNotFoundException("قرارداد پیدا نشد.");

            var contractAttachments = await GetContractAttachmentsAsync(contract.Id);

            var merchantType = (IdentityTypeEnum)Enum.Parse(typeof(IdentityTypeEnum), contract.Merchant.Type.ToString());

            var isEditable = true;

            var hasEndorsement = await _tenantMerchantContractRepository.HasEndorsement(contract.Id, contract.TenantId);
            if (hasEndorsement)
            {
                isEditable = false;
            }

            if (isEditable)
            {
                var hasTransaction = await _financialDocumentRepository.IsTenantMerchantContractUsedInTransaction(contract.Id);
                if (hasTransaction)
                {
                    isEditable = false;
                }
            }

            var nationalCartImageFront = await _attachmentService.GetDocumentObject(EntityType.TenantMerchantContract.ToString(), contractAttachments.NationalCartImageFront, contractAttachments.NationalCartImageFrontContentType);
            var nationalCartImageBack = await _attachmentService.GetDocumentObject(EntityType.TenantMerchantContract.ToString(), contractAttachments.NationalCartImageBack, contractAttachments.NationalCartImageBackContentType);
            var businessDocumentImage = await _attachmentService.GetDocumentObject(EntityType.TenantMerchantContract.ToString(), contractAttachments.BusinessDocumentImage, contractAttachments.BusinessDocumentImageContentType);
            var officialNewspaper = await _attachmentService.GetDocumentObject(EntityType.TenantMerchantContract.ToString(), contractAttachments.OfficialNewspaper, contractAttachments.OfficialNewspaperContentType); ;

            return new GetTenantMerchantContractVm
            {
                Id = contract.Id,
                TenantId = contract.TenantId,
                TenantName = contract.Tenant.Title,
                MerchantId = contract.MerchantId,
                MerchantName = contract.Merchant.Title,
                MerchantType = merchantType,
                MerchantTypeTitle = merchantType.GetEnumDescription(),
                NationalCartImageFront = nationalCartImageFront,
                NationalCartImageBack = nationalCartImageBack,
                BusinessDocumentImage = businessDocumentImage,
                OfficialNewspaper = officialNewspaper,
                BusinessDocumentType = contractAttachments.BusinessDocumentType,
                BusinessDocumentTypeTitle = contractAttachments.BusinessDocumentTypeName,
                EnamadLink = contract.EnamadLink,
                InternetBusinessLicenseLink = contract.InternetBusinessLicenseLink,
                ContractNumber = contract.ContractNumber,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                SettlementType = contract.SettlementType,
                SettlementTypeTitle = contract.SettlementType.GetEnumDescription(),
                IsCommissionExchanged = contract.IsCommissionExchanged,
                InstallmentsCount = contract.InstallmentsCount,
                CommissionDeductionMethodType = contract.CommissionDeductionMethodType,
                CommissionDeductionMethodTypeTitle = contract.CommissionDeductionMethodType == null ? null : contract.CommissionDeductionMethodType.GetEnumDescription(),
                InterestPercentage = contract.InterestPercentage,
                InterestReferenceTypes = contract.InterestReferenceTypes,
                InterestReferenceTypeTitles = (contract.InterestReferenceTypes == null || contract.InterestReferenceTypes.Count == 0) ? null : contract.InterestReferenceTypes.Select(x => x.GetEnumDescription()).ToList(),
                BillingPeriodType = contract.BillingPeriodType,
                BillingPeriodTypeTitle = contract.BillingPeriodType.GetEnumDescription(),
                BillingPeriod = contract.BillingPeriod,
                DailyBillingOriginDate = contract.DailyBillingOriginDate,
                BillingBreak = contract.BillingBreak,
                PaymentMethodType = contract.PaymentMethodType,
                PaymentMethodTypeTitle = contract.PaymentMethodType.GetEnumDescription(),
                GuaranteeType = contract.GuaranteeType,
                GuaranteeTypeTitle = contract.GuaranteeType == null ? null : contract.GuaranteeType.GetEnumDescription(),
                GuaranteeDescription = contract.GuaranteeDescription,
                CommissionCalculationType = contract.CommissionCalculationType,
                CommissionCalculationTypeTitle = contract.CommissionCalculationType.GetEnumDescription(),
                TieredCommissions = contract.TieredCommissions?.Select(x => new TieredCommissionDto()
                {
                    FromAmount = x.FromAmount,
                    ToAmount = x.ToAmount,
                    Percentage = x.Percentage,
                    MinAmount = x.MinAmount,
                    MaxAmount = x.MaxAmount
                }).ToList(),
                FixedAmountCommission = contract.FixedAmountCommission,
                FixedPercentageCommission = contract.FixedPercentageCommission,
                CommissionReferenceTypes = contract.CommissionReferenceTypes,
                CommissionReferenceTypeTitles = (contract.CommissionReferenceTypes == null || contract.CommissionReferenceTypes.Count == 0) ? null : contract.CommissionReferenceTypes.Select(x => x.GetEnumDescription()).ToList(),
                TransactionMinCommissionAmount = contract.TransactionMinCommissionAmount,
                TransactionMaxCommissionAmount = contract.TransactionMaxCommissionAmount,
                PeriodMinCommissionAmount = contract.PeriodMinCommissionAmount,
                PeriodMaxCommissionAmount = contract.PeriodMaxCommissionAmount,
                IsEditable = isEditable,
                Status = contract.Status
            };
        }

        public async Task<GetTenantMerchantDocumentNameVm> GetContractAttachmentsAsync(int contractId)
        {
            var attachment = new GetTenantMerchantDocumentNameVm();
            var contractAttachments = await _attachmentRepositoryReadOnlyRepository.GetListAsync(EntityType.TenantMerchantContract, contractId);

            if (!contractAttachments.Any()) return attachment;

            foreach (var contractAttachment in contractAttachments)
            {
                switch ((AttachmentCategory)contractAttachment.AttachmentCategory)
                {
                    case AttachmentCategory.CartMeliFront:
                        attachment.NationalCartImageFront = contractAttachment.FileReference;
                        attachment.NationalCartImageFrontContentType = contractAttachment.ContentType;
                        break;

                    case AttachmentCategory.CartMeliBack:
                        attachment.NationalCartImageBack = contractAttachment.FileReference;
                        attachment.NationalCartImageBackContentType = contractAttachment.ContentType;
                        break;

                    case AttachmentCategory.OfficialNewspaper:
                        attachment.OfficialNewspaper = contractAttachment.FileReference;
                        attachment.OfficialNewspaperContentType = contractAttachment.ContentType;
                        break;

                    case AttachmentCategory.LeaseAgreement:
                        attachment.BusinessDocumentImage = contractAttachment.FileReference;
                        attachment.BusinessDocumentImageContentType = contractAttachment.ContentType;
                        attachment.BusinessDocumentTypeName = BusinessDocumentType.LeaseAgreement.GetEnumDescription();
                        attachment.BusinessDocumentType = BusinessDocumentType.LeaseAgreement;
                        break;
                    case AttachmentCategory.PropertyDeed:
                        attachment.BusinessDocumentImage = contractAttachment.FileReference;
                        attachment.BusinessDocumentImageContentType = contractAttachment.ContentType;
                        attachment.BusinessDocumentTypeName = BusinessDocumentType.PropertyDeed.GetEnumDescription();
                        attachment.BusinessDocumentType = BusinessDocumentType.PropertyDeed;
                        break;
                    case AttachmentCategory.BusinessLicense:
                        attachment.BusinessDocumentImage = contractAttachment.FileReference;
                        attachment.BusinessDocumentImageContentType = contractAttachment.ContentType;
                        attachment.BusinessDocumentTypeName = BusinessDocumentType.BusinessLicense.GetEnumDescription();
                        attachment.BusinessDocumentType = BusinessDocumentType.BusinessLicense;
                        break;
                }
            }

            return attachment;
        }
    }
}
