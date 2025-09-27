using Application.Query.ReadOnlyRepositoryContracts;
using Application.Service.Dtos.Shared;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.TenantAggregate.Exceptions;
using MediatR;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries.IpgSettings
{
    public class GetTenantIpgSettingByWalletIdQuery : IRequest<int>
    {
        public int TenantId { get; set; }

        public GetTenantIpgSettingByWalletIdQuery(int tenantId)
        {
            TenantId = tenantId;
        }

    }

    public class GetTenantIpgSettingByWalletIdQueryHandler : IRequestHandler<GetTenantIpgSettingByWalletIdQuery, int>
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly ITenantPlatformContractReadOnlyRepository _tenantPlatformContractReadOnlyRepository;
        private readonly PublicAppConfiguration _publicAppConfiguration;

        public GetTenantIpgSettingByWalletIdQueryHandler(
            ITenantPlatformContractReadOnlyRepository tenantPlatformContractReadOnlyRepository,
            ITenantRepository tenantRepository,
            IOptions<PublicAppConfiguration> publicAppConfiguration)
        {
            _tenantPlatformContractReadOnlyRepository = tenantPlatformContractReadOnlyRepository;
            _tenantRepository = tenantRepository;
            _publicAppConfiguration = publicAppConfiguration.Value;
        }

        public async Task<int> Handle(GetTenantIpgSettingByWalletIdQuery request, CancellationToken cancellationToken)
        {
            var tenantPlatformContacts = await _tenantPlatformContractReadOnlyRepository.GetActiveContractsAsync(request.TenantId);
            if (tenantPlatformContacts.Count == 0)
            {
                var platformTenantId = _publicAppConfiguration.PlatformTenantId;

                var platformTenantIpgSetting = await _tenantRepository.TenantIPgSettingGetByTenantIdAsync(platformTenantId);
                if (platformTenantIpgSetting == null)
                    throw new TenantIpgSettingNotFoundException("تنظیمات درگاه پرداخت پلتفرم پیدا نشد.");

                return platformTenantIpgSetting.Id;
            }

            return tenantPlatformContacts.First(x => x.Status).TenantIpgSettingId;
        }

    }
}
