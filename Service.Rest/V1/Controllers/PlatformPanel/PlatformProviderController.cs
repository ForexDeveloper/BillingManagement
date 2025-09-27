using Application.Query.Queries.Providers;
using Application.Query.ViewModels.Providers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Rest.V1.RequestModels.Providers;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers.PlatformPanel
{
    [ApiVersion("1.0")]
    [Route("api/platform-panel/providers")]
    [ApiController]
    [Authorize]
    public class PlatformProviderController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PlatformProviderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetAsync))]
        [SwaggerOperation("Get a provider by id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Provider returned", typeof(GetProviderVm))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Provider not found")]
        public async Task<ActionResult<GetProviderVm>> GetAsync([FromRoute] int id)
        {
            var result = await _mediator.Send(new GetProviderByIdQuery(id));
            return Ok(result);
        }

        [HttpGet]
        [SwaggerOperation("Get providers list")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Providers list returned", typeof(GetProvidersVm))]
        public async Task<ActionResult<GetProvidersVm>> GetListAsync([FromQuery] GetProvidersModel request)
        {
            var tenants = await _mediator.Send(new GetProvidersQuery(request.ProviderType, request.PageIndex, request.PageSize, request.SortColumn,
                request.SortDirection, request.SearchValue));

            return Ok(tenants);
        }
    }
}