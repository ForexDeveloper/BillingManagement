using Application.Query.ReadOnlyRepositoryContracts;
using Application.Service.Dtos.Shared;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.TenantAggregate.Exceptions;
using MediatR;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetTenantIpgSettingByWalletIdQuery : IRequest<int>
    {
        public int TenantId { get; set; }
        public int? WalletId { get; set; }

        public GetTenantIpgSettingByWalletIdQuery(int tenantId, int? walletId)
        {
            TenantId = tenantId;
            WalletId = walletId;
        }

    }

    public class GetTenantIpgSettingByWalletIdQueryHandler : IRequestHandler<GetTenantIpgSettingByWalletIdQuery, int>
    {
        private readonly ITenantReadOnlyRepository _tenantReadOnlyRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly ITenantPlatformContractReadOnlyRepository _tenantPlatformContractReadOnlyRepository;
        private readonly IWalletContractReadOnlyRepository _walletContractReadOnlyRepository;
        private readonly IWalletReadOnlyRepository _walletReadOnlyRepository;
        private readonly PublicAppConfiguration _publicAppConfiguration;

        public GetTenantIpgSettingByWalletIdQueryHandler(ITenantReadOnlyRepository tenantReadOnlyRepository,
            ITenantPlatformContractReadOnlyRepository tenantPlatformContractReadOnlyRepository, IWalletContractReadOnlyRepository walletContractReadOnlyRepository,
            IWalletReadOnlyRepository walletReadOnlyRepository, ITenantRepository tenantRepository,
            IOptions<PublicAppConfiguration> publicAppConfiguration)
        {
            _tenantReadOnlyRepository = tenantReadOnlyRepository;
            _tenantPlatformContractReadOnlyRepository = tenantPlatformContractReadOnlyRepository;
            _walletContractReadOnlyRepository = walletContractReadOnlyRepository;
            _walletReadOnlyRepository = walletReadOnlyRepository;
            _tenantRepository = tenantRepository;
            _publicAppConfiguration = publicAppConfiguration.Value;
        }

        public async Task<int> Handle(GetTenantIpgSettingByWalletIdQuery request, CancellationToken cancellationToken)
        {
            if (!request.WalletId.HasValue)
                return await GetTenantIpgSettingFromTenantPlatformContract(request.TenantId);

            var walletContractId = await _walletReadOnlyRepository.GetWalletContractIdByWalletIdAsync(request.TenantId, request.WalletId.Value);

            if (walletContractId == null)
                return await GetTenantIpgSettingFromTenantPlatformContract(request.TenantId);

            var tenantIpgSettingId = await _walletContractReadOnlyRepository.GetTenantIpgSettingIdAsync(walletContractId.Value, request.TenantId);

            if (tenantIpgSettingId == null)
                return await GetTenantIpgSettingFromTenantPlatformContract(request.TenantId);

            return tenantIpgSettingId.Value;
        }

        private async Task<int> GetTenantIpgSettingFromTenantPlatformContract(int tenantId)
        {
            var tenantPlatformContacts = await _tenantPlatformContractReadOnlyRepository.GetActiveContractsAsync(tenantId);
            if (tenantPlatformContacts.Count == 0)
            {
                var platformTenantId = _publicAppConfiguration.PlatformTenantId;

                var platformTenantIpgSetting = await _tenantRepository.TenantIPgSettingGetByTenantIdAsync(platformTenantId);
                if (platformTenantIpgSetting == null)
                    throw new TenantIpgSettingNotFoundException("تنظیمات درگاه پرداخت پلتفرم پیدا نشد.");

                return platformTenantIpgSetting.Id;
            }

            return tenantPlatformContacts.Any(x => x.Status) ?
                tenantPlatformContacts.First(x => x.Status).TenantIpgSettingId :
                tenantPlatformContacts.MaxBy(x => x.Id).TenantIpgSettingId;
        }

    }
}
