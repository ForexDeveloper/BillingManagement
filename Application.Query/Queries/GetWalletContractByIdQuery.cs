using Application.Query.QueryModels;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.WalletContracts;
using Application.Service.Dtos.Shared;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.WalletContractAggregate;
using Domain.Core.Entities.WalletContractAggregate.Exceptions;
using Domain.Core.Helper;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetWalletContractByIdQuery : IRequest<GetWalletContractVm>
    {
        public int Id { get; }
        public int? TenantId { get; }
        public GetWalletContractByIdQuery(int id, int? tenantId = null)
        {
            Id = id;
            TenantId = tenantId;
        }
    }

    public class GetWalletContractByIdQueryHandler : IRequestHandler<GetWalletContractByIdQuery, GetWalletContractVm>
    {
        private readonly IWalletContractReadOnlyRepository _walletContractReadOnlyRepository;
        private readonly IWalletContractRepository _walletContractyRepository;
        private readonly IFinancialDocumentRepository _financialDocumentRepository;

        public GetWalletContractByIdQueryHandler(IWalletContractReadOnlyRepository walletContractReadOnlyRepository,
            IFinancialDocumentRepository financialDocumentRepository,
            IWalletContractRepository walletContractyRepository)
        {
            _walletContractReadOnlyRepository = walletContractReadOnlyRepository;
            _financialDocumentRepository = financialDocumentRepository;
            _walletContractyRepository = walletContractyRepository;
        }

        public async Task<GetWalletContractVm> Handle(GetWalletContractByIdQuery request, CancellationToken cancellationToken)
        {
            var contract = await _walletContractReadOnlyRepository.GetByIdAsync(request.Id, request.TenantId)
               ?? throw new WalletContractNotFoundException("قرارداد پیدا نشد.");

            var guarantors = contract.WalletContractGuarantors;
            var financiers = contract.WalletContractFinanciers;
            var facilitators = contract.WalletContractFacilitators;

            var guarantorVm = CreateGuarantorVm(guarantors);
            var financierVm = CreateFinancierVm(financiers);
            var facilitatorsVm = CreateFacilitatorsVm(facilitators);
            (bool isEditable, bool canAcceptOrReject) = await CanEditOrAcceptRejectContract(contract.Id, contract.TenantId, contract.RootParentId, contract.GrantingProcessId);

            return new GetWalletContractVm
            {
                Id = contract.Id,
                TenantId = contract.TenantId,
                TenantName = contract.TenantName,
                ContractNumber = contract.ContractNumber,
                Status = contract.Status,
                StatusTitle = contract.Status.GetEnumDescription(),
                OrganizationId = contract.OrganizationId,
                OrganizationTitle = contract.OrganizationTitle,
                IsEditable = isEditable,
                CanAcceptOrReject = canAcceptOrReject,
                AssignWalletToOrganizationCustomers = contract.AssignWalletToOrganizationCustomers,
                TenantIpgSettingId = contract.TenantIpgSettingId,
                TenantIpgSettingName = contract.TenantIpgSettingName,
                Guarantor = guarantorVm,
                Financier = financierVm,
                Facilitators = facilitatorsVm,
                Plans = contract.WalletContractPlans.Select(x => new WalletContractPlanVm(x.Id, x.PlanId, x.PlanTitle)).ToList(),
                Rejection = contract.Rejections?.Select(x => new WalletContractRejectionVm(x.Id, x.Reason, x.CreatedDateTime)).ToList().LastOrDefault(),
                WalletType = contract.WalletType,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
            };
        }

        private async Task<(bool isEditable, bool canAcceptOrReject)> CanEditOrAcceptRejectContract(int contractId, int tenantId, int? rootParentId, int? grantingProcessId)
        {
            var isEditable = true;
            var canAcceptOrReject = true;

            if (grantingProcessId.HasValue)
            {
                isEditable = false;
                canAcceptOrReject = false;
            }

            if (canAcceptOrReject)
            {
                var hasCashWallet = await _walletContractReadOnlyRepository.HasCashWalletContractByIdAsync(contractId);
                if (hasCashWallet)
                {
                    canAcceptOrReject = false;
                    isEditable = false;
                }
            }

            if (canAcceptOrReject)
            {
                var existsActiveOrDeactiveContract = await _walletContractReadOnlyRepository.ExistsActiveOrDeactiveContractWithIdGreaterThan(contractId, rootParentId, tenantId);
                if (existsActiveOrDeactiveContract)
                    canAcceptOrReject = false;
            }

            if (isEditable)
            {
                var hasEndorsement = await _walletContractyRepository.HasEndorsement(contractId, tenantId);
                isEditable = !hasEndorsement;
            }

            if (isEditable)
            {
                var hasTransaction = await _financialDocumentRepository.IsWalletContractUsedInTransaction(contractId);
                if (hasTransaction)
                {
                    isEditable = false;
                }
            }

            return (isEditable, canAcceptOrReject);
        }

        private WalletContractGuarantorVm CreateGuarantorVm(List<WalletContractGuarantorQueryModel> guarantors)
        {
            if (guarantors == null || guarantors.Count == 0) return null;

            var guarantor = guarantors.FirstOrDefault();

            return new WalletContractGuarantorVm
            {
                GuarantorId = guarantor.GuarantorId,
                GuarantorTitle = guarantor.GuarantorTitle,
                PortionTypes = guarantor.PortionTypes,
                PortionTypeTitles = (guarantor.PortionTypes == null || guarantor.PortionTypes.Count == 0) ? null : guarantor.PortionTypes.Select(x => x.GetEnumDescription()).ToList(),
                CommissionCalculationType = guarantor.CommissionCalculationType,
                CommissionCalculationTypeTitle = guarantor.CommissionCalculationType.GetEnumDescription(),
                FixedAmountCommission = guarantor.FixedAmountCommission,
                FixedPercentageCommission = guarantor.FixedPercentageCommission,
                TransactionMinCommissionAmount = guarantor.TransactionMinCommissionAmount,
                TransactionMaxCommissionAmount = guarantor.TransactionMaxCommissionAmount,
                PeriodMinCommissionAmount = guarantor.PeriodMinCommissionAmount,
                PeriodMaxCommissionAmount = guarantor.PeriodMaxCommissionAmount,
                TieredCommissions = guarantor.TieredCommissions != null ? guarantor.TieredCommissions.Select(x => new TieredCommissionDto(x.FromAmount, x.ToAmount, x.Percentage, x.MinAmount, x.MaxAmount)).ToList() : null,
                PaymentMethodType = guarantor.PaymentMethodType,
                PaymentMethodTypeTitle = guarantor.PaymentMethodType.GetEnumDescription()
            };
        }

        private WalletContractFinancierVm CreateFinancierVm(List<WalletContractFinancierQueryModel> financiers)
        {
            if (financiers == null || financiers.Count == 0) return null;

            var financier = financiers.FirstOrDefault();

            return new WalletContractFinancierVm
            {
                FinancierId = financier.FinancierId,
                FinancierTitle = financier.FinancierTitle,
                PortionTypes = financier.PortionTypes,
                PortionTypeTitles = (financier.PortionTypes == null || financier.PortionTypes.Count == 0) ? null : financier.PortionTypes.Select(x => x.GetEnumDescription()).ToList(),
                CommissionCalculationType = financier.CommissionCalculationType,
                CommissionCalculationTypeTitle = financier.CommissionCalculationType.GetEnumDescription(),
                FixedAmountCommission = financier.FixedAmountCommission,
                FixedPercentageCommission = financier.FixedPercentageCommission,
                TransactionMinCommissionAmount = financier.TransactionMinCommissionAmount,
                TransactionMaxCommissionAmount = financier.TransactionMaxCommissionAmount,
                PeriodMinCommissionAmount = financier.PeriodMinCommissionAmount,
                PeriodMaxCommissionAmount = financier.PeriodMaxCommissionAmount,
                TieredCommissions = financier.TieredCommissions != null ? financier.TieredCommissions.Select(x => new TieredCommissionDto(x.FromAmount, x.ToAmount, x.Percentage, x.MinAmount, x.MaxAmount)).ToList() : null,
                PaymentMethodType = financier.PaymentMethodType,
                PaymentMethodTypeTitle = financier.PaymentMethodType.GetEnumDescription()
            };
        }

        private List<WalletContractFacilitatorVm> CreateFacilitatorsVm(List<WalletContractFacilitatorQueryModel> facilitators)
        {
            if (facilitators == null || facilitators.Count == 0) return null;

            return facilitators.Select(facilitator => new WalletContractFacilitatorVm
            {
                FacilitatorId = facilitator.FacilitatorId,
                FacilitatorTitle = facilitator.FacilitatorTitle,
                PortionTypes = facilitator.PortionTypes,
                CommissionCalculationType = facilitator.CommissionCalculationType,
                CommissionCalculationTypeTitle = facilitator.CommissionCalculationType.GetEnumDescription(),
                FixedAmountCommission = facilitator.FixedAmountCommission,
                FixedPercentageCommission = facilitator.FixedPercentageCommission,
                TransactionMinCommissionAmount = facilitator.TransactionMinCommissionAmount,
                TransactionMaxCommissionAmount = facilitator.TransactionMaxCommissionAmount,
                PeriodMinCommissionAmount = facilitator.PeriodMinCommissionAmount,
                PeriodMaxCommissionAmount = facilitator.PeriodMaxCommissionAmount,
                TieredCommissions = facilitator.TieredCommissions != null ? facilitator.TieredCommissions.Select(x => new TieredCommissionDto(x.FromAmount, x.ToAmount, x.Percentage, x.MinAmount, x.MaxAmount)).ToList() : null,
                PaymentMethodType = facilitator.PaymentMethodType,
                PaymentMethodTypeTitle = facilitator.PaymentMethodType.GetEnumDescription(),
                PortionTypeTitles = (facilitator.PortionTypes == null || facilitator.PortionTypes.Count == 0) ? null : facilitator.PortionTypes.Select(x => x.GetEnumDescription()).ToList(),
            }).ToList();
        }
    }
}
