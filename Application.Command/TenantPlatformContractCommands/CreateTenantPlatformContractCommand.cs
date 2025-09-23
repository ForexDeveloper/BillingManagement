using Application.Service.Contracts;
using Application.Service.Dtos.Shared;
using Application.Service.Dtos.TenantPlatformContract;
using Domain.Core.Entities.Shared;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TenantPlatformContractAggregate;
using Domain.Core.Enums;
using Domain.Core.UnitOfWorkContracts;
using MediatR;
using Shared.IdentityServerProvider.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Command.TenantPlatformContractCommands
{
    public class CreateTenantPlatformContractCommand : IRequest<int>
    {
        public int TenantId { get; set; }
        public string ContractNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public FeeCalculationType FeeCalculationType { get; set; }
        public decimal? FixedAmount { get; set; }
        public CommissionCalculationType CommissionCalculationType { get; set; }
        public List<TieredCommissionDto> TieredCommissions { get; set; }
        public decimal? FixedAmountCommission { get; set; }
        public decimal? FixedPercentageCommission { get; set; }
        public List<CommissionReferenceType> CommissionReferenceTypes { get; set; }
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

        public CreateTenantPlatformContractCommand(int tenantId, string contractNumber, DateTime startDate, DateTime endDate,
            string description,
            FeeCalculationType feeCalculationType, CommissionCalculationType commissionCalculationType,
            List<TieredCommissionDto> tieredCommissions, decimal? fixedAmount, decimal? fixedAmountCommission,
            decimal? fixedPercentageCommission, List<CommissionReferenceType> commissionReferenceTypes,
            decimal? transactionMinCommissionAmount, decimal? transactionMaxCommissionAmount,
            decimal? periodMinCommissionAmount, decimal? periodMaxCommissionAmount,
            TimeInterval billingPeriodType, int billingPeriod, DateTime? dailyBillingOriginDate, int? gracePeriod, decimal? penaltyPercent,
            int tenantIpgSettingId, List<TenantPlatformContractFacilitatorDto> facilitators,
            List<TenantPlatformContractProviderDto> providers)
        {
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

    public class CreateTenantPlatformContractCommandHandler : IRequestHandler<CreateTenantPlatformContractCommand, int>
    {
        private readonly ITenantPlatformContractRepository _tenantPlatformContractRepository;
        private readonly IApplicationDbContextUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ITenantPlatformContractService _tenantPlatformContractService;
        public CreateTenantPlatformContractCommandHandler(
            ITenantPlatformContractRepository tenantPlatformContractRepository,
            IApplicationDbContextUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ITenantPlatformContractService tenantPlatformContractService)
        {
            _tenantPlatformContractRepository = tenantPlatformContractRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _tenantPlatformContractService = tenantPlatformContractService;
        }
        public async Task<int> Handle(CreateTenantPlatformContractCommand request, CancellationToken cancellationToken)
        {
            var isExistsActiveContract = await _tenantPlatformContractRepository.IsExistsActiveContractAsync(request.TenantId);
            if (isExistsActiveContract)
            {
                throw new ArgumentValidationException(nameof(request.TenantId), "به دلیل وجود قرارداد فعال، امکان ایجاد قرارداد جدید وجود ندارد.");
            }

            await _tenantPlatformContractService.ValidateInputData(request.TenantId, request.TenantIpgSettingId, request.Providers, request.Facilitators?.Select(x => x.FacilitatorId)?.ToList());

            var contract = new TenantPlatformContract(
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
                request.TenantIpgSettingId
            );
            contract.SetCreatorUserId(_currentUserService.UserId);
            contract.SetClientId(_currentUserService.ClientId);

            _tenantPlatformContractService.SetTenantPlatformContractFacilitators(contract, request.Facilitators);
            _tenantPlatformContractService.SetTenantPlatformContractProviders(contract, request.Providers);

            if (request.CommissionCalculationType == CommissionCalculationType.UniformTiered || request.CommissionCalculationType == CommissionCalculationType.CumulativeTiered)
            {
                var tieredCommissions = request.TieredCommissions.Select(x =>
                new TieredCommission(x.FromAmount, x.ToAmount, x.Percentage, x.MinAmount, x.MaxAmount)).ToList();
                contract.SetTieredCommissions(tieredCommissions);
            }

            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    await _tenantPlatformContractRepository.AddAsync(contract);
                    await _unitOfWork.SaveChangesAsync();
                    await _tenantPlatformContractService.PublishTenantPlatformContractAddedOrUpdatedEvent(contract, request.Providers.Select(x => x.ProviderId).ToList());
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    transaction.Complete();
                }
                catch (Exception)
                {
                    transaction.Dispose();
                    throw;
                }
            }

            return contract.Id;
        }

    }
}
