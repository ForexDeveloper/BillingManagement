using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.IpgSettings;
using Application.Service.Dtos.Shared;
using Domain.Core.Enums;
using MediatR;
using Microsoft.Extensions.Options;
using Shared.Utilities.Extensions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetTenantIpgSettingsQuery : BasePaginatedListRequest, IRequest<GetTenantIpgSettingVm>
    {
        public int? TenantId { get; set; }
        public IpgSettingOwnerType? IpgSettingOwnerType { get; set; }
        public bool? IsActive { get; set; }

        public GetTenantIpgSettingsQuery(int? tenantId, IpgSettingOwnerType? ipgSettingOwnerType, bool? isActive,
            int pageIndex, int pageSize, string? sortColumn, SortDirection? sortDirection, string? searchValue)
        {
            TenantId = tenantId;
            IpgSettingOwnerType = ipgSettingOwnerType;
            IsActive = isActive;
            PageIndex = pageIndex;
            PageSize = pageSize;
            SortColumn = sortColumn;
            SortDirection = sortDirection;
            SearchValue = searchValue;

        }

    }

    public class GetTenantIpgSettingsQueryHandler : IRequestHandler<GetTenantIpgSettingsQuery, GetTenantIpgSettingVm>
    {
        private readonly ITenantReadOnlyRepository _tenantReadOnlyRepository;
        private readonly PublicAppConfiguration _publicAppConfiguration;


        public GetTenantIpgSettingsQueryHandler(ITenantReadOnlyRepository tenantReadOnlyRepository, IOptions<PublicAppConfiguration> publicAppConfiguration)
        {
            _tenantReadOnlyRepository = tenantReadOnlyRepository;
            _publicAppConfiguration = publicAppConfiguration.Value;
        }

        public async Task<GetTenantIpgSettingVm> Handle(GetTenantIpgSettingsQuery request, CancellationToken cancellationToken)
        {
            var platformTenantId = _publicAppConfiguration.PlatformTenantId;

            var ipgSettings = await _tenantReadOnlyRepository.TenantIpSettingGetAllAsync(request, platformTenantId);

            return new GetTenantIpgSettingVm()
            {
                PageIndex = ipgSettings.PageIndex,
                PageSize = ipgSettings.PageSize,
                TotalCount = ipgSettings.TotalCount,
                Items = ipgSettings.Items.Select(x =>
                    new TenantIpgSettingVm
                    {
                        Id = x.Id,
                        Title = x.Title,
                        TenantId = x.TenantId,
                        TenantName = x.TenantName,
                        IpgType = x.IpgType,
                        IpgTypeTitle = ((IpgTypeEnum)Enum.Parse(typeof(IpgTypeEnum), x.IpgType.ToString())).GetEnumDescription(),
                        IsActive = x.IsActive,
                    }
                ).ToList()
            };

        }

    }
}
