using Application.Service.Contracts;
using Application.Service.Dtos.Shared;
using Application.Service.Dtos.TenantPlatformContracts;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.Providers;
using Domain.Core.Entities.Shared;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.TenantPlatformContractAggregate;
using Domain.Core.Entities.TenantPlatformContractAggregate.Exceptions;
using Domain.Core.Enums;
using Domain.Core.UnitOfWorkContracts;
using MediatR;
using Shared.IdentityServerProvider.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Command.TenantPlatformContractCommands
{
    public class UpdateTenantPlatformContractCommand : IRequest<int>
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string ContractNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public FeeCalculationType FeeCalculationType { get; set; }
        public decimal? FixedAmount { get; set; }
        public BmCommissionCalculationType CommissionCalculationType { get; set; }
        public List<TieredCommissionDto> TieredCommissions { get; set; }
        public decimal? FixedAmountCommission { get; set; }
        public decimal? FixedPercentageCommission { get; set; }
        public List<BmCommissionReferenceType> CommissionReferenceTypes { get; set; }
        public decimal? TransactionMinCommissionAmount { get; set; }
        public decimal? TransactionMaxCommissionAmount { get; set; }
        public decimal? PeriodMinCommissionAmount { get; set; }
        public decimal? PeriodMaxCommissionAmount { get; set; }
        public TimeInterval BillingPeriodType { get; set; }
        public int BillingPeriod { get; set; }
        public DateTime? DailyBillingOriginDate { get; set; }
        public int? GracePeriod { get; set; }
        public decimal? PenaltyPercent { get; set; }
        public int TenantIpgSettingId { get; set; }
        public List<TenantPlatformContractFacilitatorDto> Facilitators { get; set; }
        public List<TenantPlatformContractProviderDto> Providers { get; set; }

        public UpdateTenantPlatformContractCommand(int id, int tenantId, string contractNumber, DateTime startDate, DateTime endDate,
            string description,
            FeeCalculationType feeCalculationType, BmCommissionCalculationType commissionCalculationType,
            List<TieredCommissionDto> tieredCommissions, decimal? fixedAmount, decimal? fixedAmountCommission,
            decimal? fixedPercentageCommission, List<BmCommissionReferenceType> commissionReferenceTypes,
            decimal? transactionMinCommissionAmount, decimal? transactionMaxCommissionAmount,
            decimal? periodMinCommissionAmount, decimal? periodMaxCommissionAmount,
            TimeInterval billingPeriodType, int billingPeriod, DateTime? dailyBillingOriginDate, int? gracePeriod, decimal? penaltyPercent,
            int tenantIpgSettingId, List<TenantPlatformContractFacilitatorDto> facilitators,
            List<TenantPlatformContractProviderDto> providers)
        {
            Id = id;
            TenantId = tenantId;
            ContractNumber = contractNumber;
            StartDate = startDate;
            EndDate = endDate;
            Description = description;
            FeeCalculationType = feeCalculationType;
            CommissionCalculationType = commissionCalculationType;
            TieredCommissions = tieredCommissions;
            FixedAmount = fixedAmount;
            FixedAmountCommission = fixedAmountCommission;
            FixedPercentageCommission = fixedPercentageCommission;
            CommissionReferenceTypes = commissionReferenceTypes;
            TransactionMinCommissionAmount = transactionMinCommissionAmount;
            TransactionMaxCommissionAmount = transactionMaxCommissionAmount;
            PeriodMinCommissionAmount = periodMinCommissionAmount;
            PeriodMaxCommissionAmount = periodMaxCommissionAmount;
            BillingPeriodType = billingPeriodType;
            BillingPeriod = billingPeriod;
            DailyBillingOriginDate = dailyBillingOriginDate;
            GracePeriod = gracePeriod;
            PenaltyPercent = penaltyPercent;
            TenantIpgSettingId = tenantIpgSettingId;
            Facilitators = facilitators;
            Providers = providers;
        }
    }

    public class UpdateTenantPlatformContractCommandHandler : IRequestHandler<UpdateTenantPlatformContractCommand, int>
    {
        private readonly ITenantPlatformContractRepository _tenantPlatformContractRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly IApplicationDbContextUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IProviderRepository _providerRepository;
        private readonly IFinancialDocumentRepository _financialDocumentRepository;
        private readonly ITenantPlatformContractService _tenantPlatformContractService;

        public UpdateTenantPlatformContractCommandHandler(
            ITenantPlatformContractRepository tenantPlatformContractRepository,
            ITenantRepository tenantRepository, IApplicationDbContextUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IProviderRepository providerRepository,
            IFinancialDocumentRepository financialDocumentRepository,
            ITenantPlatformContractService tenantPlatformContractService)
        {
            _tenantPlatformContractRepository = tenantPlatformContractRepository;
            _tenantRepository = tenantRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _providerRepository = providerRepository;
            _financialDocumentRepository = financialDocumentRepository;
            _tenantPlatformContractService = tenantPlatformContractService;
        }
        public async Task<int> Handle(UpdateTenantPlatformContractCommand request, CancellationToken cancellationToken)
        {
            var contract = await _tenantPlatformContractRepository.GetAsync(request.Id);
            if (contract == null)
                throw new TenantPlatformContractNotFoundException("قرارداد پیدا نشد.");

            var hasAppendix = await _tenantPlatformContractRepository.HasEndorsement(contract.Id, request.TenantId);
            if (hasAppendix)
            {
                throw new TenantPlatformContractNotEditableException("ویرایش این قرارداد به دلیل وجود الحاقیه ممکن نیست.");
            }

            var hasTransaction = await _financialDocumentRepository.IsTenantPlatformContractUsedInTransaction(contract.Id);
            if (hasTransaction)
            {
                throw new TenantPlatformContractNotEditableException("ویرایش این قرارداد به دلیل وجود تراکنش ممکن نیست.");
            }

            await _tenantPlatformContractService.ValidateInputData(request.TenantId, request.TenantIpgSettingId, request.Providers, request.Facilitators?.Select(x => x.FacilitatorId)?.ToList());

            contract.Update(
                request.TenantId,
                request.ContractNumber,
                request.StartDate,
                request.EndDate,
                request.Description,
                request.FeeCalculationType,
                request.CommissionCalculationType,
                request.FixedAmount,
                request.FixedAmountCommission,
                request.FixedPercentageCommission,
                request.CommissionReferenceTypes,
                request.TransactionMinCommissionAmount,
                request.TransactionMaxCommissionAmount,
                request.PeriodMinCommissionAmount,
                request.PeriodMaxCommissionAmount,
                request.BillingPeriodType,
                request.BillingPeriod,
                request.DailyBillingOriginDate,
                request.GracePeriod,
                request.PenaltyPercent,
                request.TenantIpgSettingId);

            UpdateTenantPlatformContractTieredCommissions(contract, request);
            UpdateTenantPlatformContractFacilitators(contract, request);
            UpdateTenantPlatformContractProviders(contract, request);

            _tenantPlatformContractRepository.Update(contract);
            await _tenantPlatformContractService.PublishTenantPlatformContractAddedOrUpdatedEvent(contract, request.Providers.Select(x => x.ProviderId).ToList());
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return contract.Id;
        }

        private void UpdateTenantPlatformContractTieredCommissions(TenantPlatformContract contract, UpdateTenantPlatformContractCommand request)
        {
            if (request.CommissionCalculationType == BmCommissionCalculationType.UniformTiered || request.CommissionCalculationType == BmCommissionCalculationType.CumulativeTiered)
            {
                var tieredCommissions = request.TieredCommissions.Select(x =>
                new TieredCommission(x.FromAmount, x.ToAmount, x.Percentage, x.MinAmount, x.MaxAmount)).ToList();
                contract.SetTieredCommissions(tieredCommissions);
            }
            else
            {
                contract.SetTieredCommissions(null);
            }
        }

        private void UpdateTenantPlatformContractFacilitators(TenantPlatformContract contract, UpdateTenantPlatformContractCommand request)
        {
            List<TenantPlatformContractFacilitator> inputFacilitators = [];
            List<TenantPlatformContractFacilitator> newFacilitators = [];
            List<TenantPlatformContractFacilitator> oldFacilitators = contract.Facilitators;
            var existingCombinations = new HashSet<int>();

            if (request.Facilitators != null)
                inputFacilitators.AddRange(request.Facilitators.Select(x => new TenantPlatformContractFacilitator(x.FacilitatorId, x.FixedAmountCommissionPercentage, x.TransactionsCommissionPercentage, x.PaymentMethodType)));

            if (oldFacilitators != null)
            {
                existingCombinations = oldFacilitators
                    .Select(f => f.FacilitatorId)
                    .ToHashSet();
            }

            foreach (var input in inputFacilitators)
            {
                var combination = input.FacilitatorId;

                if (!existingCombinations.Contains(combination))
                {
                    var newFacilitator = new TenantPlatformContractFacilitator(input.FacilitatorId, input.FixedAmountCommissionPercentage, input.TransactionsCommissionPercentage, input.PaymentMethodType);
                    newFacilitators.Add(newFacilitator);
                }
                else
                {
                    if (oldFacilitators != null)
                    {
                        var facilitator = oldFacilitators.FirstOrDefault(x => x.FacilitatorId == input.FacilitatorId);
                        if (facilitator != null)
                        {
                            facilitator.Update(input.FixedAmountCommissionPercentage, input.TransactionsCommissionPercentage, input.PaymentMethodType);
                        }
                    }
                }
            }

            if (newFacilitators.Count != 0)
            {
                contract.SetFacilitators(newFacilitators);
            }

            if (oldFacilitators != null)
            {
                foreach (var oldFacilitator in oldFacilitators)
                {
                    if (inputFacilitators.All(i => i.FacilitatorId != oldFacilitator.FacilitatorId))
                    {
                        oldFacilitator.SetDeleted();
                        oldFacilitator.SetEditDateTime(DateTime.Now);
                    }
                }
            }
        }

        private void UpdateTenantPlatformContractProviders(TenantPlatformContract contract, UpdateTenantPlatformContractCommand request)
        {
            List<TenantPlatformContractProvider> inputProviders = [];
            List<TenantPlatformContractProvider> newProviders = [];
            List<TenantPlatformContractProvider> oldProviders = contract.Providers;
            var existingCombinations = new HashSet<int>();

            if (request.Providers != null)
                inputProviders.AddRange(request.Providers.Select(x => new TenantPlatformContractProvider(contract.Id, x.ProviderId, x.Amount)));

            if (oldProviders != null)
            {
                existingCombinations = oldProviders
                    .Select(f => f.ProviderId)
                    .ToHashSet();
            }


            foreach (var input in inputProviders)
            {
                var combination = input.ProviderId;

                if (!existingCombinations.Contains(combination))
                {
                    var newProvider = new TenantPlatformContractProvider(contract.Id, input.ProviderId, input.Amount);
                    newProviders.Add(newProvider);
                }
                else
                {
                    if (oldProviders != null)
                    {
                        var provider = oldProviders.FirstOrDefault(x => x.ProviderId == input.ProviderId);
                        if (provider != null && provider.Amount != input.Amount)
                        {
                            provider.SetAmount(input.Amount);
                        }
                    }
                }
            }

            if (newProviders.Any())
            {
                contract.SetProviders(newProviders);
            }

            if (oldProviders != null)
            {
                foreach (var oldProvider in oldProviders)
                {
                    if (inputProviders.All(i => i.ProviderId != oldProvider.ProviderId))
                    {
                        oldProvider.SetDeleted();
                        oldProvider.SetEditDateTime(DateTime.Now);
                    }
                }
            }

        }
    }
}
