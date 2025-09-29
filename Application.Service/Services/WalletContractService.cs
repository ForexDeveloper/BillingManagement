using Application.Service.Contracts;
using Domain.Core.Entities.FacilitatorAggregate;
using Domain.Core.Entities.FinancierAggregate;
using Domain.Core.Entities.GuarantorAggregate;
using Domain.Core.Entities.OrganizationAggregate;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.WalletContractAggregate;
using Domain.Core.Enums;
using Shared.EventBus.Events;
using System.Collections.Generic;
using System.Linq;

namespace Application.Service.Services
{
    public class WalletContractService : IWalletContractService
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IFinancierRepository _financierRepository;
        private readonly IGuarantorRepository _guarantorRepository;
        private readonly IFacilitatorRepository _facilitatorRepository;
        public WalletContractService(
            ITenantRepository tenantRepository,
            IOrganizationRepository organizationRepository,
            IFinancierRepository financierRepository,
            IGuarantorRepository guarantorRepository,
            IFacilitatorRepository facilitatorRepository)
        {
            _tenantRepository = tenantRepository;
            _organizationRepository = organizationRepository;
            _financierRepository = financierRepository;
            _guarantorRepository = guarantorRepository;
            _facilitatorRepository = facilitatorRepository;
        }

        public void SetWalletContractGuarantor(WalletContract contract, List<FcmWalletContractGuarantor> guarantors)
        {
            List<WalletContractGuarantor> guarantorsList = [];
            var guarantor = guarantors.First();
            var newGuarantor = new WalletContractGuarantor(guarantor.Id, contract.Id, guarantor.GuarantorId, guarantor.PortionTypes, guarantor.CommissionCalculationType,
                guarantor.FixedAmountCommission, guarantor.FixedPercentageCommission, guarantor.TransactionMinCommissionAmount, guarantor.TransactionMaxCommissionAmount,
                guarantor.PeriodMinCommissionAmount, guarantor.PeriodMaxCommissionAmount, guarantor.PaymentMethodType);

            if ((BmCommissionCalculationType)guarantor.CommissionCalculationType == BmCommissionCalculationType.UniformTiered ||
                (BmCommissionCalculationType)guarantor.CommissionCalculationType == BmCommissionCalculationType.CumulativeTiered)
            {
                newGuarantor.SetTieredCommissions(guarantor.TieredCommissions.Select(x => new Domain.Core.Entities.Shared.TieredCommission(x.FromAmount, x.ToAmount, x.Percentage, x.MinAmount, x.MaxAmount)).ToList());
            }

            guarantorsList.Add(newGuarantor);
            contract.SetWalletContractGuarantors(guarantorsList);
        }

        public void SetWalletContractFinancier(WalletContract contract, List<FcmWalletContractFinancier> financiers)
        {
            if (financiers == null)
            {
                return;
            }

            List<WalletContractFinancier> financiersList = [];
            var financier = financiers.First();
            var newFinancier = new WalletContractFinancier(financier.Id, contract.Id, financier.FinancierId, financier.PortionTypes, financier.CommissionCalculationType,
                financier.FixedAmountCommission, financier.FixedPercentageCommission, financier.TransactionMaxCommissionAmount, financier.TransactionMinCommissionAmount, financier.PeriodMinCommissionAmount, financier.PeriodMaxCommissionAmount, financier.PaymentMethodType);

            if ((BmCommissionCalculationType)financier.CommissionCalculationType == BmCommissionCalculationType.UniformTiered ||
                (BmCommissionCalculationType)financier.CommissionCalculationType == BmCommissionCalculationType.CumulativeTiered)
            {
                newFinancier.SetTieredCommissions(financier.TieredCommissions.Select(x => new Domain.Core.Entities.Shared.TieredCommission(x.FromAmount, x.ToAmount, x.Percentage, x.MinAmount, x.MaxAmount)).ToList());
            }

            financiersList.Add(newFinancier);
            contract.SetWalletContractFinanciers(financiersList);
        }

        public void SetWalletContractFacilitators(WalletContract contract, List<FcmWalletContractFacilitator> facilitators)
        {
            if (facilitators == null)
            {
                return;
            }

            List<WalletContractFacilitator> facilitatorsList = [];

            foreach (var facilitator in facilitators)
            {
                var newFacilitator = new WalletContractFacilitator(facilitator.Id, contract.Id, facilitator.FacilitatorId, facilitator.PortionTypes, facilitator.CommissionCalculationType,
                    facilitator.FixedAmountCommission, facilitator.FixedPercentageCommission, facilitator.TransactionMinCommissionAmount, facilitator.TransactionMaxCommissionAmount, facilitator.PeriodMinCommissionAmount, facilitator.PeriodMaxCommissionAmount, facilitator.PaymentMethodType);

                if ((BmCommissionCalculationType)facilitator.CommissionCalculationType == BmCommissionCalculationType.UniformTiered ||
                    (BmCommissionCalculationType)facilitator.CommissionCalculationType == BmCommissionCalculationType.CumulativeTiered)
                {
                    newFacilitator.SetTieredCommissions(facilitator.TieredCommissions.Select(x => new Domain.Core.Entities.Shared.TieredCommission(x.FromAmount, x.ToAmount, x.Percentage, x.MinAmount, x.MaxAmount)).ToList());
                }
                facilitatorsList.Add(newFacilitator);
            }

            contract.SetWalletContractFacilitators(facilitatorsList);
        }

    }
}
