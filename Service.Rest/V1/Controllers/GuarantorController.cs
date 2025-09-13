using Application.Query.Queries;
using Application.Query.ViewModels.Guarantors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Rest.V1.RequestModels.Guarantors;
using Shared.IdentityServerProvider.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers;

[ApiVersion("1.0")]
[Route("api/tenant-panel/guarantors")]
[ApiController]
[Authorize]
public class GuarantorController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public GuarantorController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HttpGet("{id}")]
    [ActionName(nameof(GetAsync))]
    [SwaggerOperation("Get a tenant guarantor by id")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Tenant guarantor returned", typeof(GetGuarantorVm))]
    [SwaggerResponse((int)HttpStatusCode.NotFound, "Tenant guarantor not found")]
    public async Task<ActionResult<GetGuarantorVm>> GetAsync([FromRoute] int id)
    {
        var result = await _mediator.Send(new GetGuarantorByIdQuery(id, _currentUserService.TenantId));
        return Ok(result);
    }

    [HttpGet]
    [SwaggerOperation("Get tenant guarantors list")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Tenant guarantors list returned", typeof(GetGuarantorsVm))]
    public async Task<ActionResult<GetGuarantorsVm>> GetListAsync([FromQuery] GetTenantGuarantorsModel request)
    {
        var tenants = await _mediator.Send(new GetGuarantorsQuery(_currentUserService.TenantId, request.PageIndex, request.PageSize, request.SortColumn,
            request.SortDirection, request.SearchValue, request.PersonType));

        return Ok(tenants);
    }
}