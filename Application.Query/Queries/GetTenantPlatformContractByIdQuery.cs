using Application.Query.ViewModels.TenantPlatfromContracts;
using Application.Service.Dtos.Shared;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.TenantPlatformContractAggregate;
using Domain.Core.Entities.TenantPlatformContractAggregate.Exceptions;
using Domain.Core.Enums;
using Domain.Core.Helper;
using MediatR;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetTenantPlatformContractByIdQuery : IRequest<GetTenantPlatformContractVm>
    {
        public int Id { get; }
        public GetTenantPlatformContractByIdQuery(int id)
        {
            Id = id;
        }
    }

    public class GetTenantPlatformContractByIdQueryHandler : IRequestHandler<GetTenantPlatformContractByIdQuery, GetTenantPlatformContractVm>
    {
        private readonly ITenantPlatformContractRepository _tenantPlatformContractRepository;
        private readonly PublicAppConfiguration _publicAppConfiguration;
        private readonly IFinancialDocumentRepository _financialDocumentRepository;

        public GetTenantPlatformContractByIdQueryHandler(
            ITenantPlatformContractRepository tenantPlatformContractRepository,
            IFinancialDocumentRepository financialDocumentRepository,
            IOptions<PublicAppConfiguration> publicAppConfiguration)
        {
            _tenantPlatformContractRepository = tenantPlatformContractRepository;
            _financialDocumentRepository = financialDocumentRepository;
            _publicAppConfiguration = publicAppConfiguration.Value;
        }

        public async Task<GetTenantPlatformContractVm> Handle(GetTenantPlatformContractByIdQuery request, CancellationToken cancellationToken)
        {
            var contract = await _tenantPlatformContractRepository.GetAsync(request.Id)
                ?? throw new TenantPlatformContractNotFoundException("قرارداد پیدا نشد.");

            var platformTenantId = _publicAppConfiguration.PlatformTenantId;

            var isEditable = true;
            var hasTransaction = await _financialDocumentRepository.IsTenantPlatformContractUsedInTransaction(contract.Id);
            if (hasTransaction)
            {
                isEditable = false;
            }

            return new GetTenantPlatformContractVm
            {
                Id = contract.Id,
                TenantId = contract.TenantId,
                TenantName = contract.Tenant.Title,
                ContractNumber = contract.ContractNumber,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                Description = contract.Description,
                FeeCalculationType = contract.FeeCalculationType,
                FeeCalculationTypeTitle = contract.FeeCalculationType.GetEnumDescription(),
                CommissionCalculationType = contract.CommissionCalculationType,
                CommissionCalculationTypeTitle = contract.CommissionCalculationType.GetEnumDescription(),
                FixedAmount = contract.FixedAmount,
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
                TransactionMinCommissionAmount = contract.TransactionMinCommissionAmount,
                TransactionMaxCommissionAmount = contract.TransactionMaxCommissionAmount,
                PeriodMinCommissionAmount = contract.PeriodMinCommissionAmount,
                PeriodMaxCommissionAmount = contract.PeriodMaxCommissionAmount,
                BillingPeriodType = contract.BillingPeriodType,
                BillingPeriodTypeTitle = contract.BillingPeriodType.GetEnumDescription(),
                BillingPeriod = contract.BillingPeriod,
                DailyBillingOriginDate = contract.DailyBillingOriginDate,
                GracePeriod = contract.GracePeriod,
                PenaltyPercent = contract.PenaltyPercent,
                Facilitators = contract.Facilitators?.Select(x => new TenantPlatformContractFacilitatorVm()
                {
                    FacilitatorId = x.FacilitatorId,
                    FacilitatorName = x.Facilitator.Name,
                    FixedAmountCommissionPercentage = x.FixedAmountCommissionPercentage,
                    TransactionsCommissionPercentage = x.TransactionsCommissionPercentage,
                    PaymentMethodType = x.PaymentMethodType,
                    PaymentMethodTypeTitle = x.PaymentMethodType?.GetEnumDescription()
                }).ToList(),
                Providers = contract.Providers?.Select(x => new TenantPlatformContractProviderVm()
                {
                    Amount = x.Amount,
                    ProviderId = x.ProviderId,
                    ProviderName = x.Provider.Name,
                    ProviderType = x.Provider.ProviderType,
                    ProviderTypeTitle = x.Provider.ProviderType.GetEnumDescription(),
                    ProviderEnglishName = x.Provider.EnglishName
                }).ToList(),
                TenantIpgSettingId = contract.TenantIpgSettingId,
                IpgSettingOwnerType = platformTenantId == contract.TenantIpgSettings.TenantId ? IpgSettingOwnerType.Platform : IpgSettingOwnerType.Tenant,
                IpgSettingOwnerTypeTitle = platformTenantId == contract.TenantIpgSettings.TenantId ? IpgSettingOwnerType.Platform.GetEnumDescription() : IpgSettingOwnerType.Tenant.GetEnumDescription(),
                TenantIpgSettingName = contract.TenantIpgSettings?.Title,
                IsEditable = contract.Status && isEditable,
                Status = contract.Status
            };
        }

    }
}
