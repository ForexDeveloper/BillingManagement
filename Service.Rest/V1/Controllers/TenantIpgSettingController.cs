using Application.Query.Queries.IpgSettings;
using Application.Query.ViewModels.IpgSettings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Rest.V1.RequestModels.IpgSettings;
using Shared.IdentityServerProvider.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/tenant-panel/tenant-ipg-settings")]
    [ApiController]
    [Authorize]
    public class TenantIpgSettingController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;

        public TenantIpgSettingController(IMediator mediator, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetAsync))]
        [SwaggerOperation("Get tenant ipg setting by id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Tenant ipg setting returned", typeof(TenantIpgSettingVm))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Tenant ipg setting not found")]
        public async Task<ActionResult<TenantIpgSettingVm>> GetAsync([FromRoute] int id)
        {
            var ipgSetting = await _mediator.Send(new GetTenantIpgSettingByIdQuery(id, _currentUserService.TenantId));
            return Ok(ipgSetting);
        }

        [HttpGet]
        [ActionName(nameof(GetListAsync))]
        [SwaggerOperation("Get tenant ipg settings list")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Tenant ipg setting list returned", typeof(List<TenantIpgSettingVm>))]
        public async Task<ActionResult<List<TenantIpgSettingVm>>> GetListAsync([FromQuery] GetTenantIpgSettingsBaseModel request)
        {
            var ipgSettings = await _mediator.Send(new GetTenantIpgSettingsQuery(
                _currentUserService.TenantId,
                request.IpgSettingOwnerType,
                request.IsActive,
                request.PageIndex,
                request.PageSize,
                request.SortColumn,
                request.SortDirection,
                request.SearchValue
                ));

            return Ok(ipgSettings);
        }

        [HttpGet("fetch-ipg-setting-id")]
        [ActionName(nameof(GetIpgSettingIdAsync))]
        [SwaggerOperation("Get tenant ipg setting by wallet id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Tenant ipg setting returned", typeof(int))]
        public async Task<ActionResult<int>> GetIpgSettingIdAsync()
        {
            var tenantIpgSettingId = await _mediator.Send(new GetTenantIpgSettingByWalletIdQuery(
                _currentUserService.TenantId
            ));

            return Ok(tenantIpgSettingId);
        }
    }
}
