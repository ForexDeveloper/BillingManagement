using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.IpgSettings;
using Domain.Core.Entities.TenantAggregate.Exceptions;
using Domain.Core.Enums;
using Domain.Core.Helper;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetTenantIpgSettingByIdQuery : IRequest<TenantIpgSettingVm>
    {
        public int Id { get; }
        public int? TenantId { get; }
        public GetTenantIpgSettingByIdQuery(int id, int? tenantId = null)
        {
            Id = id;
            TenantId = tenantId;
        }
    }

    public class GetTenantIpgSettingByIdQueryHandler : IRequestHandler<GetTenantIpgSettingByIdQuery, TenantIpgSettingVm>
    {
        private readonly ITenantReadOnlyRepository _tenantReadOnlyRepository;

        public GetTenantIpgSettingByIdQueryHandler(ITenantReadOnlyRepository tenantReadOnlyRepository)
        {
            _tenantReadOnlyRepository = tenantReadOnlyRepository;
        }

        public async Task<TenantIpgSettingVm> Handle(GetTenantIpgSettingByIdQuery request, CancellationToken cancellationToken)
        {
            var ipgSetting = await _tenantReadOnlyRepository.TenantIpSettingGetAsync(request.Id, request.TenantId)
                ?? throw new TenantIpgSettingNotFoundException("تنظیمات درگاه پرداخت پیدا نشد.");

            return new TenantIpgSettingVm
            {
                Id = ipgSetting.Id,
                Title = ipgSetting.Title,
                TenantId = ipgSetting.TenantId,
                TenantName = ipgSetting.Tenant.Title,
                IpgType = ipgSetting.IpgType,
                IpgTypeTitle = ((IpgTypeEnum)Enum.Parse(typeof(IpgTypeEnum), ipgSetting.IpgType.ToString())).GetEnumDescription(),
                IsActive = ipgSetting.IsActive,
            };
        }

    }
}
