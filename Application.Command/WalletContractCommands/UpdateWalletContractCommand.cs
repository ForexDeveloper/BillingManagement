using Application.Query.ReadOnlyRepositoryContracts;
using Application.Service.Contracts;
using Application.Service.Dtos.WalletContract;
using Domain.Core.Entities;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.Shared;
using Domain.Core.Entities.TenantMerchantContractAggregate.Exceptions;
using Domain.Core.Entities.WalletContractAggregate;
using Domain.Core.Entities.WalletContractAggregate.Exceptions;
using Domain.Core.Enums;
using Domain.Core.UnitOfWorkContracts;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Command.WalletContractCommands
{
    public class UpdateWalletContractCommand : IRequest<int>
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public List<int> PlanIds { get; set; }
        public int OrganizationId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? TenantIpgSettingId { get; set; }
        public bool AssignWalletToOrganizationCustomers { get; set; }
        public List<int>? Customers { get; set; }
        public WalletContractGuarantorDto Guarantor { get; set; }
        public WalletContractFinancierDto? Financier { get; set; }
        public List<WalletContractFacilitatorDto>? Facilitators { get; set; }

        public UpdateWalletContractCommand(int id, int tenantId, List<int> planIds, int organizationId, DateTime startDate, DateTime endDate, int? tenantIpgSettingId,
            bool assignWalletToOrganizationCustomers,
            WalletContractGuarantorDto guarantor, List<int>? customers, WalletContractFinancierDto? financier,
            List<WalletContractFacilitatorDto>? facilitators)
        {
            Id = id;
            TenantId = tenantId;
            PlanIds = planIds;
            OrganizationId = organizationId;
            StartDate = startDate;
            EndDate = endDate;
            TenantIpgSettingId = tenantIpgSettingId;
            Customers = customers;
            Guarantor = guarantor;
            Financier = financier;
            Facilitators = facilitators;
            AssignWalletToOrganizationCustomers = assignWalletToOrganizationCustomers;
        }
    }

    public class UpdatedWalletContractCommandHandler : IRequestHandler<UpdateWalletContractCommand, int>
    {
        private readonly IWalletContractRepository _walletContractRepository;
        private readonly IApplicationDbContextUnitOfWork _unitOfWork;
        private readonly IFinancialDocumentRepository _financialDocumentRepository;
        private readonly IWalletContractService _walletContractService;
        private readonly IWalletContractReadOnlyRepository _walletContractReadOnlyRepository;

        public UpdatedWalletContractCommandHandler(
            IApplicationDbContextUnitOfWork unitOfWork,
            IWalletContractRepository walletContractRepository,
            IFinancialDocumentRepository financialDocumentRepository, IWalletContractService walletContractService,
            IWalletContractReadOnlyRepository walletContractReadOnlyRepository = null)
        {
            _unitOfWork = unitOfWork;
            _walletContractRepository = walletContractRepository;
            _financialDocumentRepository = financialDocumentRepository;
            _walletContractService = walletContractService;
            _walletContractReadOnlyRepository = walletContractReadOnlyRepository;
        }

        public async Task<int> Handle(UpdateWalletContractCommand request, CancellationToken cancellationToken)
        {
            var contract = await _walletContractRepository.GetAsync(request.Id, request.TenantId) ?? throw new WalletContractNotFoundException("قرارداد پیدا نشد.");

            if (request.TenantId != contract.TenantId)
            {
                throw new TenantForbiddenException();
            }

            if (contract.GrantingProcessId.HasValue)
            {
                throw new WalletContractNotEditableException("ویرایش این قرارداد ممکن نیست.");
            }

            if (contract.Status == WalletContractStatus.Reject)
            {
                throw new WalletContractNotEditableException("ویرایش این قرارداد به دلیل وضعیت رد شده ممکن نیست.");
            }

            var hasCashWallet = await _walletContractReadOnlyRepository.HasCashWalletContractByIdAsync(request.Id);
            if (hasCashWallet)
            {
                throw new WalletContractNotEditableException("ویرایش این قرارداد ممکن نیست.");
            }

            var hasEndorsement = await _walletContractRepository.HasEndorsement(contract.Id, request.TenantId);
            if (hasEndorsement)
            {
                throw new WalletContractNotEditableException("ویرایش این قرارداد به دلیل وجود الحاقیه ممکن نیست.");
            }

            var hasTransaction = await _financialDocumentRepository.IsWalletContractUsedInTransaction(contract.Id);
            if (hasTransaction)
            {
                throw new WalletContractNotEditableException("ویرایش این قرارداد به دلیل وجود تراکنش ممکن نیست.");
            }

            await _walletContractService.ValidateInputData(request.TenantId, request.OrganizationId, request.PlanIds, request.Guarantor, request.Financier, request.Facilitators, request.Customers, request.TenantIpgSettingId);

            contract.UpdateWalletContract(
                    request.TenantIpgSettingId,
                    request.StartDate,
                    request.EndDate
                );

            UpdateWalletContractPlans(contract, request.PlanIds);
            UpdateWalletContractGuarantor(contract, request.Guarantor);
            UpdateWalletContractFinancier(contract, request.Financier);
            UpdateWalletContractFacilitators(contract, request.Facilitators);

            _walletContractRepository.Update(contract);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return contract.Id;
        }

        private void UpdateWalletContractPlans(WalletContract contract, List<int> plansId)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));
            if (plansId == null) throw new ArgumentNullException(nameof(plansId));

            var oldContractPlans = contract.WalletContractPlans;
            var newContractPlans = new List<WalletContractPlan>();

            foreach (var planId in plansId)
            {
                if (oldContractPlans.All(x => x.PlanId != planId))
                {
                    newContractPlans.Add(new WalletContractPlan(contract.Id, planId));
                }
            }

            foreach (var oldContractPlan in oldContractPlans)
            {
                if (!plansId.Contains(oldContractPlan.PlanId))
                {
                    oldContractPlan.SetDeleted();
                    oldContractPlan.SetEditDateTime(DateTime.Now);
                }
            }

            contract.SetWalletContractPlans(newContractPlans);
        }

        private void UpdateWalletContractGuarantor(WalletContract contract, WalletContractGuarantorDto guarantor)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));
            if (guarantor == null) throw new ArgumentNullException(nameof(guarantor));

            List<WalletContractGuarantor> inputGuarantorList = [];
            List<WalletContractGuarantor> newGuarantorList = [];
            List<WalletContractGuarantor> oldGuarantorList = contract.WalletContractGuarantors;
            List<WalletContractGuarantor> updatedGuarantorList = [];

            var inputGuarantor = new WalletContractGuarantor(contract.Id, guarantor.GuarantorId, guarantor.PortionTypes, guarantor.CommissionCalculationType,
                guarantor.FixedAmountCommission, guarantor.FixedPercentageCommission, guarantor.TransactionMinCommissionAmount,
                guarantor.TransactionMaxCommissionAmount, guarantor.PeriodMinCommissionAmount, guarantor.PeriodMaxCommissionAmount, guarantor.PaymentMethodType);

            if (guarantor.TieredCommissions != null && guarantor.TieredCommissions.Any())
            {
                inputGuarantor.SetTieredCommissions(guarantor.TieredCommissions.Select(x => new TieredCommission(x.FromAmount, x.ToAmount, x.Percentage, x.MinAmount, x.MaxAmount)).ToList());
            }

            inputGuarantorList.Add(inputGuarantor);

            var existingGuarantorIds = oldGuarantorList.Select(f => (f.GuarantorId)).ToHashSet();

            foreach (var input in inputGuarantorList)
            {
                if (existingGuarantorIds.Contains(input.GuarantorId))
                {
                    var guarantorToUpdate = oldGuarantorList.First(f => f.GuarantorId == input.GuarantorId);

                    guarantorToUpdate.Update(input.PortionTypes, input.CommissionCalculationType, input.FixedAmountCommission, input.FixedPercentageCommission, input.TransactionMinCommissionAmount,
                        input.TransactionMaxCommissionAmount, input.PeriodMinCommissionAmount, input.PeriodMaxCommissionAmount, input.PaymentMethodType);
                    guarantorToUpdate.ClearTieredCommissions();

                    if (input.CommissionCalculationType == CommissionCalculationType.UniformTiered ||
                        input.CommissionCalculationType == CommissionCalculationType.CumulativeTiered)
                    {
                        guarantorToUpdate.SetTieredCommissions(input.TieredCommissions);
                    }
                    guarantorToUpdate.SetEditDateTime(DateTime.Now);
                }
                else
                {
                    var newGuarantor = new WalletContractGuarantor(contract.Id, input.GuarantorId, input.PortionTypes, input.CommissionCalculationType,
                        input.FixedAmountCommission, input.FixedPercentageCommission, input.TransactionMinCommissionAmount, input.TransactionMaxCommissionAmount, input.PeriodMinCommissionAmount, input.PeriodMaxCommissionAmount, input.PaymentMethodType);

                    if (input.CommissionCalculationType == CommissionCalculationType.UniformTiered ||
                        input.CommissionCalculationType == CommissionCalculationType.CumulativeTiered)
                    {
                        newGuarantor.SetTieredCommissions(input.TieredCommissions);
                    }

                    newGuarantorList.Add(newGuarantor);
                }
            }

            contract.SetWalletContractGuarantors(newGuarantorList);

            foreach (var oldGuarantor in oldGuarantorList)
            {
                if (!inputGuarantorList.Any(i => i.GuarantorId == oldGuarantor.GuarantorId))
                {
                    oldGuarantor.SetDeleted();
                    oldGuarantor.SetEditDateTime(DateTime.Now);
                }
            }
        }

        private void UpdateWalletContractFinancier(WalletContract contract, WalletContractFinancierDto? financier)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));

            if (financier == null)
            {
                foreach (var walletContractFinancier in contract.WalletContractFinanciers)
                {
                    walletContractFinancier.SetDeleted();
                    walletContractFinancier.SetEditDateTime(DateTime.Now);
                }
                return;
            }

            if (contract.WalletContractFinanciers == null)
            {
                List<WalletContractFinancier> financierList = [];

                var newFinancier = new WalletContractFinancier(contract.Id, financier.FinancierId, financier.PortionTypes, financier.CommissionCalculationType, financier.FixedAmountCommission, financier.FixedPercentageCommission,
                    financier.TransactionMinCommissionAmount, financier.TransactionMaxCommissionAmount, financier.PeriodMinCommissionAmount, financier.PeriodMaxCommissionAmount, financier.PaymentMethodType);

                if (financier.TieredCommissions != null && financier.TieredCommissions.Any())
                {
                    newFinancier.SetTieredCommissions(financier.TieredCommissions
                        .Select(x => new TieredCommission(x.FromAmount, x.ToAmount, x.Percentage, x.MinAmount, x.MaxAmount)).ToList());
                }

                financierList.Add(newFinancier);
                contract.SetWalletContractFinanciers(financierList);

                return;
            }

            List<WalletContractFinancier> inputFinancierList = [];
            List<WalletContractFinancier> newFinancierList = [];
            List<WalletContractFinancier> oldFinancierList = contract.WalletContractFinanciers;

            var inputFinancier = new WalletContractFinancier(contract.Id, financier.FinancierId, financier.PortionTypes, financier.CommissionCalculationType,
               financier.FixedAmountCommission, financier.FixedPercentageCommission, financier.TransactionMinCommissionAmount,
               financier.TransactionMaxCommissionAmount, financier.PeriodMinCommissionAmount, financier.PeriodMaxCommissionAmount, financier.PaymentMethodType);

            if (financier.TieredCommissions != null && financier.TieredCommissions.Any())
            {
                inputFinancier.SetTieredCommissions(financier.TieredCommissions.Select(x => new TieredCommission(x.FromAmount, x.ToAmount, x.Percentage, x.MinAmount, x.MaxAmount)).ToList());
            }

            inputFinancierList.Add(inputFinancier);

            var existingFinancierIds = oldFinancierList.Select(f => f.FinancierId).ToHashSet();

            foreach (var input in inputFinancierList)
            {
                if (existingFinancierIds.Contains(input.FinancierId))
                {
                    var financierToUpdate = oldFinancierList.First(f => f.FinancierId == input.FinancierId);

                    financierToUpdate.Update(input.PortionTypes, input.CommissionCalculationType, input.FixedAmountCommission, input.FixedPercentageCommission,
                        input.TransactionMinCommissionAmount, input.TransactionMaxCommissionAmount, input.PeriodMinCommissionAmount, input.PeriodMaxCommissionAmount, input.PaymentMethodType);

                    financierToUpdate.ClearTieredCommissions();

                    if (input.CommissionCalculationType == CommissionCalculationType.UniformTiered ||
                        input.CommissionCalculationType == CommissionCalculationType.CumulativeTiered)
                    {
                        financierToUpdate.SetTieredCommissions(input.TieredCommissions);
                    }
                    financierToUpdate.SetEditDateTime(DateTime.Now);
                }
                else
                {
                    var newFinancier = new WalletContractFinancier(contract.Id, input.FinancierId, input.PortionTypes, input.CommissionCalculationType,
                        input.FixedAmountCommission, input.FixedPercentageCommission, input.TransactionMinCommissionAmount, input.TransactionMaxCommissionAmount,
                        input.PeriodMinCommissionAmount, input.PeriodMaxCommissionAmount, input.PaymentMethodType);

                    if (input.CommissionCalculationType == CommissionCalculationType.UniformTiered ||
                       input.CommissionCalculationType == CommissionCalculationType.CumulativeTiered)
                    {
                        newFinancier.SetTieredCommissions(input.TieredCommissions);
                    }
                    newFinancierList.Add(newFinancier);
                }
            }

            if (newFinancierList.Any())
            {
                contract.SetWalletContractFinanciers(newFinancierList);
            }

            foreach (var oldFinancier in oldFinancierList)
            {
                if (!inputFinancierList.Any(i => i.FinancierId == oldFinancier.FinancierId))
                {
                    oldFinancier.SetDeleted();
                    oldFinancier.SetEditDateTime(DateTime.Now);
                }
            }
        }

        private void UpdateWalletContractFacilitators(WalletContract contract, List<WalletContractFacilitatorDto>? facilitators)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));

            if (facilitators == null || !facilitators.Any())
            {
                foreach (var walletContractFacilitator in contract.WalletContractFacilitators)
                {
                    walletContractFacilitator.SetDeleted();
                    walletContractFacilitator.SetEditDateTime(DateTime.Now);
                }

                return;
            }

            if (contract.WalletContractFacilitators == null)
            {
                List<WalletContractFacilitator> facilitatorList = [];
                foreach (var facilitator in facilitators)
                {
                    var newFacilitator = new WalletContractFacilitator(contract.Id, facilitator.FacilitatorId, facilitator.PortionTypes,
                        facilitator.CommissionCalculationType, facilitator.FixedAmountCommission, facilitator.FixedPercentageCommission,
                        facilitator.TransactionMinCommissionAmount, facilitator.TransactionMaxCommissionAmount, facilitator.PeriodMinCommissionAmount, facilitator.PeriodMaxCommissionAmount, facilitator.PaymentMethodType);

                    if (facilitator.CommissionCalculationType == CommissionCalculationType.UniformTiered ||
                        facilitator.CommissionCalculationType == CommissionCalculationType.CumulativeTiered)
                    {
                        newFacilitator.SetTieredCommissions(facilitator.TieredCommissions.Select(x => new TieredCommission(x.FromAmount, x.ToAmount, x.Percentage, x.MinAmount, x.MaxAmount)).ToList());
                    }

                    facilitatorList.Add(newFacilitator);
                }

                contract.SetWalletContractFacilitators(facilitatorList);

                return;
            }

            List<WalletContractFacilitator> inputFacilitatorList = [];
            List<WalletContractFacilitator> newFacilitatorList = [];
            List<WalletContractFacilitator> oldFacilitatorList = contract.WalletContractFacilitators;

            foreach (var facilitator in facilitators)
            {
                var inputFacilitator = new WalletContractFacilitator(contract.Id, facilitator.FacilitatorId, facilitator.PortionTypes, facilitator.CommissionCalculationType, facilitator.FixedAmountCommission,
                    facilitator.FixedPercentageCommission, facilitator.TransactionMinCommissionAmount, facilitator.TransactionMaxCommissionAmount, facilitator.PeriodMinCommissionAmount, facilitator.PeriodMaxCommissionAmount, facilitator.PaymentMethodType);

                if (facilitator.TieredCommissions != null && facilitator.TieredCommissions.Any())
                {
                    inputFacilitator.SetTieredCommissions(facilitator.TieredCommissions.Select(x => new TieredCommission(x.FromAmount, x.ToAmount, x.Percentage, x.MinAmount, x.MaxAmount)).ToList());
                }

                inputFacilitatorList.Add(inputFacilitator);
            }

            var existingFacilitatorIds = oldFacilitatorList.Select(f => f.FacilitatorId).ToHashSet();

            foreach (var input in inputFacilitatorList)
            {
                if (existingFacilitatorIds.Contains(input.FacilitatorId))
                {
                    var facilitatorToUpdate = oldFacilitatorList.First(f => f.FacilitatorId == input.FacilitatorId);
                    facilitatorToUpdate.Update(input.PortionTypes, input.CommissionCalculationType, input.FixedAmountCommission, input.FixedPercentageCommission,
                        input.TransactionMinCommissionAmount, input.TransactionMaxCommissionAmount, input.PeriodMinCommissionAmount, input.PeriodMaxCommissionAmount, input.PaymentMethodType);

                    facilitatorToUpdate.ClearTieredCommissions();

                    if (input.CommissionCalculationType == CommissionCalculationType.UniformTiered ||
                        input.CommissionCalculationType == CommissionCalculationType.CumulativeTiered)
                    {
                        facilitatorToUpdate.SetTieredCommissions(input.TieredCommissions);
                    }
                    facilitatorToUpdate.SetEditDateTime(DateTime.Now);
                }
                else
                {
                    var newFacilitator = new WalletContractFacilitator(contract.Id, input.FacilitatorId, input.PortionTypes, input.CommissionCalculationType,
                        input.FixedAmountCommission, input.FixedPercentageCommission, input.TransactionMinCommissionAmount, input.TransactionMaxCommissionAmount,
                        input.PeriodMinCommissionAmount, input.PeriodMaxCommissionAmount, input.PaymentMethodType);

                    if (input.CommissionCalculationType == CommissionCalculationType.UniformTiered ||
                        input.CommissionCalculationType == CommissionCalculationType.CumulativeTiered)
                    {
                        newFacilitator.SetTieredCommissions(input.TieredCommissions);
                    }

                    newFacilitatorList.Add(newFacilitator);
                }
            }

            if (newFacilitatorList.Any())
            {
                contract.SetWalletContractFacilitators(newFacilitatorList);
            }

            foreach (var oldFacilitator in oldFacilitatorList)
            {
                if (!inputFacilitatorList.Any(i => i.FacilitatorId == oldFacilitator.FacilitatorId))
                {
                    oldFacilitator.SetDeleted();
                    oldFacilitator.SetEditDateTime(DateTime.Now);
                }
            }
        }
    }
}
