using Application.Service.Contracts;
using Application.Service.Dtos.Shared;
using Application.Service.Dtos.TenantMerchantContracts;
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

namespace Application.Command.TenantMerchantContractCommands
{
    public class CloneTenantMerchantContractCommand : IRequest<int>
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

        public CloneTenantMerchantContractCommand(
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

    public class CloneTenantMerchantContractCommandHandler : IRequestHandler<CloneTenantMerchantContractCommand, int>
    {
        private readonly ITenantMerchantContractRepository _tenantMerchantContractRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly IMerchantRepository _merchantRepository;
        private readonly IApplicationDbContextUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly ITenantMerchantContractService _tenantMerchantContractService;


        public CloneTenantMerchantContractCommandHandler(
            ITenantRepository tenantRepository,
            IApplicationDbContextUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IMerchantRepository merchantRepository,
            ITenantMerchantContractRepository tenantMerchantContractRepository, IAttachmentRepository attachmentRepository,
            ITenantMerchantContractService tenantMerchantContractService)
        {
            _tenantRepository = tenantRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _merchantRepository = merchantRepository;
            _tenantMerchantContractRepository = tenantMerchantContractRepository;
            _attachmentRepository = attachmentRepository;
            _tenantMerchantContractService = tenantMerchantContractService;
        }

        public async Task<int> Handle(CloneTenantMerchantContractCommand request, CancellationToken cancellationToken)
        {
            var oldContract = await _tenantMerchantContractRepository.GetAsync(request.Id);

            if (oldContract is null)
                throw new TenantMerchantContractNotFoundException("قرارداد پیدا نشد.");

            await _tenantMerchantContractService.ValidateInputData(request.TenantId, request.MerchantId, request.ContractDocument);

            var contract = await CloneTenantMerchantContract(request, oldContract.ContractNumber);
            contract.SetStatus(oldContract.Status);
            contract.SetParentId(oldContract.Id);
            contract.SetCreatorUserId(_currentUserService.UserId);
            contract.SetClientId(_currentUserService.ClientId);
            oldContract.SetStatus(false);

            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    await DeActiveOldContract(oldContract);
                    _tenantMerchantContractRepository.Update(oldContract);

                    await _tenantMerchantContractRepository.AddAsync(contract);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    var attachments = await _tenantMerchantContractService.SetTenantMerchantContractAttachments(
                        contract.Id,
                        request.ContractDocument,
                        _currentUserService.UserId,
                        _currentUserService.ClientId
                    );

                    await _attachmentRepository.AddRangeAsync(attachments);

                    _tenantMerchantContractService.PublishTenantMerchantContractAddedOrUpdatedEvent(contract);
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

        private async Task<TenantMerchantContract> CloneTenantMerchantContract(CloneTenantMerchantContractCommand request, string contractNumber)
        {
            var merchant = await _merchantRepository.GetAsync(request.MerchantId, request.TenantId);

            var contract = new TenantMerchantContract(
                    request.TenantId,
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

            _tenantMerchantContractService.SetTenantMerchantContractDocument(request.ContractDocument, merchant.Type);

            contract.SetEnamadLink(request.ContractDocument.EnamadLink);
            contract.SetInternetBusinessLicenseLink(request.ContractDocument.InternetBusinessLicenseLink);

            return contract;
        }

        private async Task DeActiveOldContract(TenantMerchantContract request)
        {
            request.SetStatus(false);
            request.SetEditDateTime(DateTime.Now);

            var contractAttachments = await _attachmentRepository.GetListAsync(request.Id, (byte)EntityType.TenantMerchantContract);

            foreach (var attachment in contractAttachments)
            {
                attachment.SetEditDateTime(DateTime.Now);
            }

        }
    }
}
