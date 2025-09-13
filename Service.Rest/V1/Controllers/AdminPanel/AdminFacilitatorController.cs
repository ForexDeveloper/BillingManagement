using Application.Query.Queries;
using Application.Query.ViewModels.Facilitators;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Rest.V1.RequestModels.Facilitators;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers.AdminPanel;

[ApiVersion("1.0")]
[Route("api/admin-panel/facilitators")]
[ApiController]
[Authorize]
public class AdminFacilitatorController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminFacilitatorController(IMediator mediator)
    {
        _mediator = mediator;
    }


    [HttpGet("{id}")]
    [ActionName(nameof(GetAsync))]
    [SwaggerOperation("Get a facilitator by id")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Facilitator returned", typeof(GetFacilitatorVm))]
    [SwaggerResponse((int)HttpStatusCode.NotFound, "Facilitator not found")]
    public async Task<ActionResult<GetFacilitatorVm>> GetAsync([FromRoute] int id)
    {
        var result = await _mediator.Send(new GetFacilitatorByIdQuery(id));
        return Ok(result);
    }

    [HttpGet]
    [SwaggerOperation("Get facilitator list")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Facilitators list returned", typeof(GetFacilitatorsVm))]
    public async Task<ActionResult<GetFacilitatorsVm>> GetListAsync([FromQuery] GetFacilitatorsModel request)
    {
        var facilitators = await _mediator.Send(new GetFacilitatorsQuery(request.TenantId, request.PageIndex, request.PageSize, request.SortColumn,
            request.SortDirection, request.SearchValue, request.PersonType));

        return Ok(facilitators);
    }
}