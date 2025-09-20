using Application.Service.Contracts;
using Application.Service.Dtos.Shared;
using Application.Service.Dtos.TenantPlatformContract;
using Domain.Core.Entities.FacilitatorAggregate;
using Domain.Core.Entities.Providers;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.TenantPlatformContractAggregate;
using Microsoft.Extensions.Options;
using Shared.EventBus.Contracts;
using Shared.EventBus.Events;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Service.Services
{
    public class TenantPlatformContractService : ITenantPlatformContractService
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly IProviderRepository _providerRepository;
        private readonly IFacilitatorRepository _facilitatorRepository;
        private readonly PublicAppConfiguration _publicAppConfiguration;
        private readonly IOutboxService _outboxService;
        public TenantPlatformContractService(ITenantRepository tenantRepository,
            IOptions<PublicAppConfiguration> publicAppConfiguration,
            IProviderRepository providerRepository, IFacilitatorRepository facilitatorRepository, IOutboxService outboxService)
        {
            _tenantRepository = tenantRepository;
            _publicAppConfiguration = publicAppConfiguration.Value;
            _providerRepository = providerRepository;
            _facilitatorRepository = facilitatorRepository;
            _outboxService = outboxService;
        }

        public void SetTenantPlatformContractFacilitators(TenantPlatformContract contract, List<TenantPlatformContractFacilitatorDto> facilitators)
        {
            if (facilitators == null || facilitators.Count == 0) return;

            List<TenantPlatformContractFacilitator> facilitatorsList = [];
            facilitatorsList.AddRange(facilitators.Select(x => new TenantPlatformContractFacilitator(x.FacilitatorId, x.FixedAmountCommissionPercentage, x.TransactionsCommissionPercentage, x.PaymentMethodType)));

            contract.SetFacilitators(facilitatorsList);
        }

        public void SetTenantPlatformContractProviders(TenantPlatformContract contract, List<TenantPlatformContractProviderDto> providers)
        {
            if (providers == null || providers.Count == 0) return;

            List<TenantPlatformContractProvider> providerList = [];
            providerList.AddRange(providers.Select(x => new TenantPlatformContractProvider(contract.Id, x.ProviderId, x.Amount)));

            contract.SetProviders(providerList);
        }

        public async Task ValidateInputData(int tenantId, int tenantIpgSettingId, List<TenantPlatformContractProviderDto> providers, List<int> facilitatorIds)
        {
            var tenant = await _tenantRepository.GetAsync(tenantId);
            if (tenant is null)
                throw new ArgumentValidationException(nameof(tenantId), "مالک زیر ساخت پیدا نشد.");

            var platformTenantId = _publicAppConfiguration.PlatformTenantId;

            if (facilitatorIds != null && facilitatorIds.Count != 0)
            {
                if (!await _facilitatorRepository.FacilitatorsBelongToTenantAsync(facilitatorIds, platformTenantId))
                {
                    throw new ArgumentValidationException("Facilitators", "تسهیلگر انتخاب شده به مالک زیر ساخت تعلق ندارد.");
                }
            }

            var ipgSetting = await _tenantRepository.TenantIPgSettingGetAsync(tenantIpgSettingId);
            if (ipgSetting is null)
            {
                throw new ArgumentValidationException(nameof(tenantIpgSettingId), "تنظیمات درگاه پرداخت پیدا نشد.");
            }

            if (providers != null && providers.Count != 0)
            {
                var provider = await _providerRepository.IsProviderExistsAsync(providers.Select(x => x.ProviderId).ToList());
                if (!provider)
                {
                    throw new ArgumentValidationException(nameof(providers), "سرویس مورد نظر پیدا نشد.");
                }
            }
        }

        public async Task PublishTenantPlatformContractAddedOrUpdatedEvent(TenantPlatformContract contract, List<int> providerIds)
        {
            var providers = await _providerRepository.GetProvidersAsync(providerIds);

            _outboxService.AddNewEvent(new BmTenantPlatformContractAddedOrUpdatedEvent()
            {
                Id = contract.Id,
                EditDateTime = contract.EditDateTime,
                TenantId = contract.TenantId,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                Status = contract.Status,
                Providers = providers.ToDictionary(x => (byte)x.ProviderType, x => x.EnglishName)
            });
        }
    }
}
