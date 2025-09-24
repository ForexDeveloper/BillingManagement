using Application.Service.Dtos.TenantPlatformContracts;
using Domain.Core.Entities.TenantPlatformContractAggregate;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Service.Contracts
{
    public interface ITenantPlatformContractService
    {
        void SetTenantPlatformContractFacilitators(TenantPlatformContract contract, List<TenantPlatformContractFacilitatorDto> facilitators);
        void SetTenantPlatformContractProviders(TenantPlatformContract contract, List<TenantPlatformContractProviderDto> providers);
        Task ValidateInputData(int tenantId, int tenantIpgSettingId, List<TenantPlatformContractProviderDto> providers, List<int> facilitatorIds);
        Task PublishTenantPlatformContractAddedOrUpdatedEvent(TenantPlatformContract contract, List<int> providerIds);
    }
}
