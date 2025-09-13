using Application.Service.Contracts;
using Application.Service.Dtos.WalletContract;
using Domain.Core.Entities.CustomerAggregate;
using Domain.Core.Entities.FacilitatorAggregate;
using Domain.Core.Entities.FinancierAggregate;
using Domain.Core.Entities.GuarantorAggregate;
using Domain.Core.Entities.OrganizationAggregate;
using Domain.Core.Entities.PlanAggregate;
using Domain.Core.Entities.Shared;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.TenantAggregate.Exceptions;
using Domain.Core.Entities.WalletContractAggregate;
using Domain.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Service.Services
{
    public class WalletContractService : IWalletContractService
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IPlanRepository _planRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IFinancierRepository _financierRepository;
        private readonly IGuarantorRepository _guarantorRepository;
        private readonly IFacilitatorRepository _facilitatorRepository;
        public WalletContractService(
            ITenantRepository tenantRepository,
            IOrganizationRepository organizationRepository,
            IPlanRepository planRepository,
            ICustomerRepository customerRepository,
            IFinancierRepository financierRepository,
            IGuarantorRepository guarantorRepository,
            IFacilitatorRepository facilitatorRepository)
        {
            _tenantRepository = tenantRepository;
            _organizationRepository = organizationRepository;
            _planRepository = planRepository;
            _customerRepository = customerRepository;
            _financierRepository = financierRepository;
            _guarantorRepository = guarantorRepository;
            _facilitatorRepository = facilitatorRepository;
        }

        public async Task ValidateInputData(int tenantId, int organizationId, List<int> planIds,
            WalletContractGuarantorDto guarantor, WalletContractFinancierDto? financier, List<WalletContractFacilitatorDto>? facilitators,
            List<int>? customers, int? TenantIpgSettingId)
        {
            var tenant = await _tenantRepository.GetAsync(tenantId);
            if (tenant is null)
                throw new ArgumentValidationException(nameof(tenantId), "مالک زیر ساخت پیدا نشد.");

            if (TenantIpgSettingId.HasValue)
            {
                var ipgSetting = await _tenantRepository.TenantIPgSettingGetAsync(TenantIpgSettingId.Value);
                if (ipgSetting == null)
                    throw new TenantIpgSettingNotFoundException("تنظیمات درگاه پرداخت پیدا نشد.");

                if (ipgSetting.TenantId != tenantId)
                    throw new ArgumentValidationException(nameof(tenantId), "تظیمات درگاه پرداخت به مالک زیر ساخت تعلق ندارد.");
            }

            var organization = await _organizationRepository.GetAsync(organizationId);
            if (organization is null)
                throw new ArgumentValidationException(nameof(organizationId), "سازمان پیدا نشد.");

            if (organization.TenantId != tenantId)
                throw new ArgumentValidationException(nameof(organizationId), "سازمان انتخاب شده به مالک زیر ساخت تعلق ندارد.");

            var plansBelongToTenantAsync = await _planRepository.CheckPlansBelongToTenantAsync(planIds, tenantId);
            if (!plansBelongToTenantAsync)
                throw new ArgumentValidationException(nameof(planIds), "پلن پیدا نشد.");

            if (customers != null && customers.Count > 0)
            {
                var customersBelongToOrganization = await _customerRepository.CustomersBelongToOrganizationAsync(customers, organizationId);
                if (!customersBelongToOrganization)
                {
                    throw new ArgumentValidationException(nameof(customers), "کاربران انتخاب شده به سازمان انتخابی، تعلق ندارند.");
                }
            }

            if (!await _guarantorRepository.IsGuarantorBelongToTenantAsync(tenantId, guarantor.GuarantorId))
            {
                throw new ArgumentValidationException(nameof(guarantor.GuarantorId), "ضامن انتخاب شده به مالک زیر ساخت تعلق ندارد.");
            }

            if (financier != null && !await _financierRepository.IsFinancierBelongToTenantAsync(tenantId, financier.FinancierId))
            {
                throw new ArgumentValidationException(nameof(financier.FinancierId), "تامین کننده مالی انتخاب شده به مالک زیر ساخت تعلق ندارد.");
            }

            if (facilitators != null && facilitators.Count > 0 && !await _facilitatorRepository.FacilitatorsBelongToTenantAsync(facilitators.Select(x => x.FacilitatorId).ToList(), tenantId))
            {
                throw new ArgumentValidationException(nameof(facilitators), "تسهیلگر انتخاب شده به مالک زیر ساخت تعلق ندارد.");
            }
        }

        public void SetWalletContractPlans(WalletContract contract, List<int>? plansId)
        {
            if (plansId == null || plansId.Count == 0)
            {
                return;
            }

            List<WalletContractPlan> planList = [];
            planList.AddRange(plansId.Select(customerId => new WalletContractPlan(contract.Id, customerId)));

            contract.SetWalletContractPlans(planList);
        }

        public void SetWalletContractCustomers(WalletContract contract, List<int>? customersId)
        {
            if (customersId == null || customersId.Count == 0)
            {
                return;
            }

            List<WalletContractBusinessIdentity> customerList = [];
            customerList.AddRange(customersId.Select(customerId => new WalletContractBusinessIdentity(contract.Id, customerId)));

            contract.SetWalletContractBusinessIdentities(customerList);
        }

        public void SetWalletContractGuarantor(WalletContract contract, WalletContractGuarantorDto guarantors)
        {
            List<WalletContractGuarantor> guarantorsList = [];

            var guarantor = new WalletContractGuarantor(contract.Id, guarantors.GuarantorId, guarantors.PortionTypes, guarantors.CommissionCalculationType,
                guarantors.FixedAmountCommission, guarantors.FixedPercentageCommission, guarantors.TransactionMinCommissionAmount, guarantors.TransactionMaxCommissionAmount, guarantors.PeriodMinCommissionAmount, guarantors.PeriodMaxCommissionAmount, guarantors.PaymentMethodType);

            if (guarantor.CommissionCalculationType == CommissionCalculationType.UniformTiered ||
                guarantor.CommissionCalculationType == CommissionCalculationType.CumulativeTiered)
            {
                guarantor.SetTieredCommissions(guarantors.TieredCommissions.Select(x => new TieredCommission(x.FromAmount, x.ToAmount, x.Percentage, x.MinAmount, x.MaxAmount)).ToList());
            }

            guarantorsList.Add(guarantor);
            contract.SetWalletContractGuarantors(guarantorsList);
        }

        public void SetWalletContractFinancier(WalletContract contract, WalletContractFinancierDto? financier)
        {
            if (financier == null)
            {
                return;
            }

            List<WalletContractFinancier> financiersList = [];

            var newFinancier = new WalletContractFinancier(contract.Id, financier.FinancierId, financier.PortionTypes, financier.CommissionCalculationType,
                financier.FixedAmountCommission, financier.FixedPercentageCommission, financier.TransactionMaxCommissionAmount, financier.TransactionMinCommissionAmount, financier.PeriodMinCommissionAmount, financier.PeriodMaxCommissionAmount, financier.PaymentMethodType);

            if (financier.CommissionCalculationType == CommissionCalculationType.UniformTiered ||
                financier.CommissionCalculationType == CommissionCalculationType.CumulativeTiered)
            {
                newFinancier.SetTieredCommissions(financier.TieredCommissions.Select(x => new TieredCommission(x.FromAmount, x.ToAmount, x.Percentage, x.MinAmount, x.MaxAmount)).ToList());
            }

            financiersList.Add(newFinancier);
            contract.SetWalletContractFinanciers(financiersList);
        }

        public void SetWalletContractFacilitators(WalletContract contract, List<WalletContractFacilitatorDto>? facilitators)
        {
            if (facilitators == null)
            {
                return;
            }

            List<WalletContractFacilitator> facilitatorsList = [];

            foreach (var facilitator in facilitators)
            {
                var newFacilitator = new WalletContractFacilitator(contract.Id, facilitator.FacilitatorId, facilitator.PortionTypes, facilitator.CommissionCalculationType,
                    facilitator.FixedAmountCommission, facilitator.FixedPercentageCommission, facilitator.TransactionMinCommissionAmount, facilitator.TransactionMaxCommissionAmount, facilitator.PeriodMinCommissionAmount, facilitator.PeriodMaxCommissionAmount, facilitator.PaymentMethodType);

                if (facilitator.CommissionCalculationType == CommissionCalculationType.UniformTiered ||
                    facilitator.CommissionCalculationType == CommissionCalculationType.CumulativeTiered)
                {
                    newFacilitator.SetTieredCommissions(facilitator.TieredCommissions.Select(x => new TieredCommission(x.FromAmount, x.ToAmount, x.Percentage, x.MinAmount, x.MaxAmount)).ToList());
                }
                facilitatorsList.Add(newFacilitator);
            }

            contract.SetWalletContractFacilitators(facilitatorsList);
        }

        public string CreateWalletContractDisplayEndDate(DateTime? endDate)
        {
            if (endDate == null)
            {
                return string.Empty;
            }

            decimal totalDays = (endDate.Value - DateTime.Today).Days;

            switch (totalDays)
            {
                case < 0:
                    return "تمام شده";
                case 0:
                    return "امروز";
                case > 0 and < 31:
                    return $"{totalDays} روز";
                case >= 31 and < 365:
                    {
                        var totalMonths = Math.Ceiling(totalDays / 31);
                        return $"{totalMonths} ماه";
                    }

                case >= 365:
                    {
                        var totalYears = Math.Ceiling(totalDays / 365);
                        return $"{totalYears} سال";
                    }
            }
        }
    }
}
