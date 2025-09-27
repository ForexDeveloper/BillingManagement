using Application.Query.Queries.Merchants;
using Application.Query.ViewModels.Merchants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers.AdminPanel
{
    [ApiVersion("1.0")]
    [Route("api/admin-panel/merchants")]
    [ApiController]
    [Authorize]
    public class AdminMerchantController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AdminMerchantController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("{tenantId}")]
        [ActionName(nameof(GetByTenantIdAsync))]
        [SwaggerOperation("get by tenant id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "merchant returned", typeof(GetMerchantListVm))]
        public async Task<ActionResult<GetMerchantListVm>> GetByTenantIdAsync([FromRoute] int tenantId)
    => Ok(await _mediator.Send(new GetMerchantListQuery(tenantId)));


    }
}
