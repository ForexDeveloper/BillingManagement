using Application.Service.Dtos.WalletContract;
using Domain.Core.Entities.WalletContractAggregate;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Service.Contracts;
public interface IWalletContractService
{
    Task ValidateInputData(int tenantId, int organizationId, List<int> planIds,
            WalletContractGuarantorDto guarantor, WalletContractFinancierDto? financier, List<WalletContractFacilitatorDto>? facilitators,
            List<int>? customers, int? TenantIpgSettingId);

    void SetWalletContractPlans(WalletContract contract, List<int>? plansId);
    void SetWalletContractCustomers(WalletContract contract, List<int>? customersId);
    void SetWalletContractGuarantor(WalletContract contract, WalletContractGuarantorDto guarantors);
    void SetWalletContractFinancier(WalletContract contract, WalletContractFinancierDto? financier);
    void SetWalletContractFacilitators(WalletContract contract, List<WalletContractFacilitatorDto>? facilitators);
    string CreateWalletContractDisplayEndDate(DateTime? endDate);
}
