using Application.Query.Queries.Facilitators;
using Application.Query.ViewModels.Facilitators;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Rest.V1.RequestModels.Facilitators;
using Shared.IdentityServerProvider.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers;

[ApiVersion("1.0")]
[Route("api/tenant-panel/facilitators")]
[ApiController]
[Authorize]
public class FacilitatorController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public FacilitatorController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }


    [HttpGet("{id}")]
    [ActionName(nameof(GetAsync))]
    [SwaggerOperation("Get a tenant facilitator by id")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Tenant facilitator returned", typeof(GetFacilitatorVm))]
    [SwaggerResponse((int)HttpStatusCode.NotFound, "Tenant facilitator not found")]
    public async Task<ActionResult<GetFacilitatorVm>> GetAsync([FromRoute] int id)
    {
        var result = await _mediator.Send(new GetFacilitatorByIdQuery(id, _currentUserService.TenantId));
        return Ok(result);
    }

    [HttpGet]
    [SwaggerOperation("Get tenant facilitators list")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Tenant facilitators list returned", typeof(GetFacilitatorsVm))]
    public async Task<ActionResult<GetFacilitatorsVm>> GetListAsync([FromQuery] GetTenantFacilitatorsModel request)
    {
        var facilitators = await _mediator.Send(new GetFacilitatorsQuery(_currentUserService.TenantId, request.PageIndex, request.PageSize, request.SortColumn,
            request.SortDirection, request.SearchValue, request.PersonType));

        return Ok(facilitators);
    }
}