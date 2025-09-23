using Application.Query.Queries;
using Application.Query.ViewModels.IpgSettings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Rest.V1.RequestModels.IpgSettings;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers.PlatformPanel
{
    [ApiVersion("1.0")]
    [Route("api/platform-panel/tenant-ipg-settings")]
    [ApiController]
    [Authorize]
    public class PlatformTenantIpgSettingController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PlatformTenantIpgSettingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetAsync))]
        [SwaggerOperation("Get tenant ipg setting by id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Tenant ipg setting returned", typeof(TenantIpgSettingVm))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Tenant ipg setting not found")]
        public async Task<ActionResult<TenantIpgSettingVm>> GetAsync([FromRoute] int id)
        {
            var ipgSetting = await _mediator.Send(new GetTenantIpgSettingByIdQuery(id));
            return Ok(ipgSetting);
        }

        [HttpGet]
        [ActionName(nameof(GetListAsync))]
        [SwaggerOperation("Get tenant ipg settings list")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Tenant ipg setting list returned", typeof(GetTenantIpgSettingVm))]
        public async Task<ActionResult<GetTenantIpgSettingVm>> GetListAsync([FromQuery] GetTenantIpgSettingsModel request)
        {
            var ipgSettings = await _mediator.Send(new GetTenantIpgSettingsQuery(
                request.TenantId,
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
    }
}
