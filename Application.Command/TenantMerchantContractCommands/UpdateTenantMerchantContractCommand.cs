using Application.Service.Contracts;
using Application.Service.Dtos.Shared;
using Application.Service.Dtos.TenantMerchantContract;
using Domain.Core.Entities;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.MerchantAggregate;
using Domain.Core.Entities.Shared;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate.Exceptions;
using Domain.Core.Enums;
using Domain.Core.UnitOfWorkContracts;
using MediatR;
using Shared.IdentityServerProvider.Contracts;
using Shared.MinIO.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Attachment = Shared.MinIO.Entities.Attachment;

namespace Application.Command.TenantMerchantContractCommands;

public class UpdateTenantMerchantContractCommand : IRequest<int>
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int MerchantId { get; set; }
    public TenantMerchantContractDocumentDto ContractDocument { get; set; }
    public string ContractNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public SettlementType SettlementType { get; set; }
    public bool IsCommissionExchanged { get; set; }
    public int? InstallmentsCount { get; set; }
    public CommissionDeductionMethodType? CommissionDeductionMethodType { get; set; }
    public decimal? InterestPercentage { get; set; }
    public List<InterestReferenceType> InterestReferenceTypes { get; set; }
    public TimeInterval BillingPeriodType { get; set; }
    public int BillingPeriod { get; set; }
    public DateTime? DailyBillingOriginDate { get; set; }
    public int? BillingBreak { get; set; }
    public PaymentMethodType PaymentMethodType { get; set; }
    public GuaranteeType? GuaranteeType { get; set; }
    public string GuaranteeDescription { get; set; }
    public CommissionCalculationType CommissionCalculationType { get; set; }
    public List<TieredCommissionDto> TieredCommissions { get; set; } = [];
    public decimal? FixedAmountCommission { get; set; }
    public decimal? FixedPercentageCommission { get; set; }
    public List<CommissionReferenceType> CommissionReferenceTypes { get; set; }
    public decimal? TransactionMinCommissionAmount { get; set; }
    public decimal? TransactionMaxCommissionAmount { get; set; }
    public decimal? PeriodMinCommissionAmount { get; set; }
    public decimal? PeriodMaxCommissionAmount { get; set; }

    public UpdateTenantMerchantContractCommand(
            int id, int tenantId, int merchantId,
            TenantMerchantContractDocumentDto contractDocument,
            string contractNumber, DateTime startDate, DateTime endDate,
            SettlementType settlementType, bool isCommissionExchanged,
            int? installmentsCount, CommissionDeductionMethodType? commissionDeductionMethodType,
            decimal? interestPercentage, List<InterestReferenceType> interestReferenceTypes,
            TimeInterval billingPeriodType, int billingPeriod, DateTime? dailyBillingOriginDate,
            int? billingBreak, PaymentMethodType paymentMethodType, GuaranteeType? guaranteeType,
            string guaranteeDescription, CommissionCalculationType commissionCalculationType,
            List<TieredCommissionDto> tieredCommissions, decimal? fixedAmountCommission,
            decimal? fixedPercentageCommission, List<CommissionReferenceType> commissionReferenceTypes,
            decimal? transactionMinCommissionAmount, decimal? transactionMaxCommissionAmount,
            decimal? periodMinCommissionAmount, decimal? periodMaxCommissionAmount)
    {
        Id = id;
        TenantId = tenantId;
        MerchantId = merchantId;
        ContractDocument = contractDocument;
        ContractNumber = contractNumber;
        StartDate = startDate;
        EndDate = endDate;
        SettlementType = settlementType;
        IsCommissionExchanged = isCommissionExchanged;
        InstallmentsCount = installmentsCount;
        CommissionDeductionMethodType = commissionDeductionMethodType;
        InterestPercentage = interestPercentage;
        InterestReferenceTypes = interestReferenceTypes;
        BillingPeriodType = billingPeriodType;
        BillingPeriod = billingPeriod;
        DailyBillingOriginDate = dailyBillingOriginDate;
        BillingBreak = billingBreak;
        PaymentMethodType = paymentMethodType;
        GuaranteeType = guaranteeType;
        GuaranteeDescription = guaranteeDescription;
        CommissionCalculationType = commissionCalculationType;
        TieredCommissions = tieredCommissions;
        FixedAmountCommission = fixedAmountCommission;
        FixedPercentageCommission = fixedPercentageCommission;
        CommissionReferenceTypes = commissionReferenceTypes;
        TransactionMinCommissionAmount = transactionMinCommissionAmount;
        TransactionMaxCommissionAmount = transactionMaxCommissionAmount;
        PeriodMinCommissionAmount = periodMinCommissionAmount;
        PeriodMaxCommissionAmount = periodMaxCommissionAmount;
    }
}

public class UpdateTenantMerchantContractCommandHandler : IRequestHandler<UpdateTenantMerchantContractCommand, int>
{
    private readonly ITenantMerchantContractRepository _tenantMerchantContractRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IMerchantRepository _merchantRepository;
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IFileManagerService _fileManagerService;
    private readonly ITenantMerchantContractService _tenantMerchantContractService;
    private readonly IFinancialDocumentRepository _financialDocumentRepository;

    public UpdateTenantMerchantContractCommandHandler(
        ITenantRepository tenantRepository,
        IApplicationDbContextUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMerchantRepository merchantRepository,
        ITenantMerchantContractRepository tenantMerchantContractRepository, IAttachmentRepository attachmentRepository,
        IFileManagerService fileManagerService, ITenantMerchantContractService tenantMerchantContractService, IFinancialDocumentRepository financialDocumentRepository)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _merchantRepository = merchantRepository;
        _tenantMerchantContractRepository = tenantMerchantContractRepository;
        _attachmentRepository = attachmentRepository;
        _fileManagerService = fileManagerService;
        _tenantMerchantContractService = tenantMerchantContractService;
        _financialDocumentRepository = financialDocumentRepository;
    }

    public async Task<int> Handle(UpdateTenantMerchantContractCommand request, CancellationToken cancellationToken)
    {
        var contract = await _tenantMerchantContractRepository.GetAsync(request.Id) ?? throw new TenantMerchantContractNotFoundException("قرارداد پیدا نشد.");

        if (request.TenantId != contract.TenantId)
        {
            throw new TenantForbiddenException();
        }

        var hasEndorsement = await _tenantMerchantContractRepository.HasEndorsement(contract.Id, request.TenantId);
        if (hasEndorsement)
        {
            throw new TenantMerchantContractNotEditableException("ویرایش این قرارداد به دلیل وجود الحاقیه ممکن نیست.");
        }

        var hasTransaction = await _financialDocumentRepository.IsTenantMerchantContractUsedInTransaction(contract.Id);
        if (hasTransaction)
        {
            throw new TenantMerchantContractNotEditableException("ویرایش این قرارداد به دلیل وجود تراکنش ممکن نیست.");
        }

        await _tenantMerchantContractService.ValidateInputData(request.TenantId, request.MerchantId, request.ContractDocument);

        contract = await UpdateTenantMerchantContract(contract, request);
        contract.SetClientId(_currentUserService.ClientId);

        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            _tenantMerchantContractRepository.Update(contract);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var oldAndNewAttachments = await SetTenantMerchantContractAttachment(
                contract.Id,
                request.ContractDocument.BusinessDocumentType,
                request.ContractDocument.NationalCartImageFront?.ToString(),
                request.ContractDocument.NationalCartImageBack?.ToString(),
                request.ContractDocument.BusinessDocumentImage?.ToString(),
                request.ContractDocument.OfficialNewspaper?.ToString(),
                _currentUserService.UserId,
                _currentUserService.ClientId);

            if (oldAndNewAttachments.newAttachments.Any())
            {
                await _attachmentRepository.AddRangeAsync(oldAndNewAttachments.newAttachments);
            }

            if (oldAndNewAttachments.oldAttachments.Any())
            {
                var oldAttachmentsDeleted = oldAndNewAttachments.oldAttachments.Where(x => x.IsDeleted == true).ToList();
                if (oldAttachmentsDeleted.Any())
                    _attachmentRepository.UpdateRange(oldAttachmentsDeleted);
            }

            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception)
            {
                transaction.Dispose();
                throw;
            }

            transaction.Complete();
        }

        return contract.Id;
    }

    private async Task<TenantMerchantContract> UpdateTenantMerchantContract(TenantMerchantContract contract, UpdateTenantMerchantContractCommand request)
    {
        var merchant = await _merchantRepository.GetAsync(request.MerchantId, request.TenantId);

        contract.Update(
                    contract.TenantId,
                    request.MerchantId,
                    request.ContractNumber,
                    request.StartDate,
                    request.EndDate,
                    request.SettlementType,
                    request.IsCommissionExchanged,
                    request.InstallmentsCount,
                    request.CommissionDeductionMethodType,
                    request.InterestPercentage,
                    request.InterestReferenceTypes,
                    request.BillingPeriodType,
                    request.BillingPeriod,
                    request.DailyBillingOriginDate,
                    request.BillingBreak,
                    request.PaymentMethodType,
                    request.GuaranteeType,
                    request.GuaranteeDescription,
                    request.CommissionCalculationType,
                    request.FixedAmountCommission,
                    request.FixedPercentageCommission,
                    request.CommissionReferenceTypes,
                    request.TransactionMinCommissionAmount,
                    request.TransactionMaxCommissionAmount,
                    request.PeriodMinCommissionAmount,
                    request.PeriodMaxCommissionAmount
            );

        if (request.CommissionCalculationType == CommissionCalculationType.UniformTiered || request.CommissionCalculationType == CommissionCalculationType.CumulativeTiered)
        {
            var tieredCommissions = request.TieredCommissions.Select(x =>
            new TieredCommission(x.FromAmount, x.ToAmount, x.Percentage, x.MinAmount, x.MaxAmount)).ToList();
            contract.SetTieredCommissions(tieredCommissions);
        }
        else
        {
            contract.SetTieredCommissions(null);
        }

        _tenantMerchantContractService.SetTenantMerchantContractDocument(request.ContractDocument, merchant.Type);

        contract.SetEnamadLink(request.ContractDocument.EnamadLink);
        contract.SetInternetBusinessLicenseLink(request.ContractDocument.InternetBusinessLicenseLink);
        contract.SetEditDateTime(DateTime.Now);

        return contract;
    }

    private async Task<(List<Attachment> oldAttachments, List<Attachment> newAttachments)> SetTenantMerchantContractAttachment(int entityId, BusinessDocumentType? businessDocumentType,
        string nationalCartImageFront, string nationalCartImageBack, string businessDocumentImage, string officialNewspaper
        , string userId, string clientId)
    {
        var oldAttachments = await _attachmentRepository.GetListAsync(entityId, (byte)EntityType.TenantMerchantContract);

        var newAttachments = new List<Attachment>();

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
            if (oldAttachments.All(x => x.FileReference != mapping.Key))
            {
                var objectInfo = await _fileManagerService.GetObjectInfo(mapping.Key, EntityType.TenantMerchantContract.ToString());
                var attachment = _tenantMerchantContractService.FillAttachmentList(objectInfo, entityId, documentMappings[mapping.Key], userId, clientId);
                newAttachments.Add(attachment);
            }
        }

        foreach (var oldAttachment in oldAttachments)
        {
            if (!documentMappings.ContainsKey(oldAttachment.FileReference))
            {
                oldAttachment.SetDeleted();
                oldAttachment.SetEditDateTime(DateTime.Now);
            }
        }

        return (oldAttachments, newAttachments);
    }
}
