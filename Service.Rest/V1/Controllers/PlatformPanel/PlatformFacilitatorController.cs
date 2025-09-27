using Application.Query.Queries.Facilitators;
using Application.Query.ViewModels.Facilitators;
using Application.Service.Dtos.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers.PlatformPanel;

[ApiVersion("1.0")]
[Route("api/platform-panel/facilitators")]
[ApiController]
[Authorize]
public class PlatformFacilitatorController : ControllerBase
{

    private readonly IMediator _mediator;
    private readonly PublicAppConfiguration _publicAppConfiguration;

    public PlatformFacilitatorController(IMediator mediator, IOptions<PublicAppConfiguration> publicAppConfiguration)
    {
        _mediator = mediator;
        _publicAppConfiguration = publicAppConfiguration.Value;
    }

    [HttpGet("lookup")]
    [SwaggerOperation("Get facilitator lookup items returned")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Facilitators lookup items returned", typeof(LookupItemVm))]
    public async Task<ActionResult<List<LookupItemVm>>> GetFacilitatorLookUpAsync()
    {
        var facilitators = await _mediator.Send(new GetFacilitatorsLookupQuery(_publicAppConfiguration.PlatformTenantId));

        return Ok(facilitators);
    }
}