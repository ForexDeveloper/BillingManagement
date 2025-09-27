using Application.Query.Queries.Guarantors;
using Application.Query.ViewModels.Guarantors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Rest.V1.RequestModels.Guarantors;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers.AdminPanel;

[ApiVersion("1.0")]
[Route("api/admin-panel/guarantors")]
[ApiController]
[Authorize]
public class AdminGuarantorController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminGuarantorController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    [ActionName(nameof(GetAsync))]
    [SwaggerOperation("Get a guarantor by id")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Guarantor returned", typeof(GetGuarantorVm))]
    [SwaggerResponse((int)HttpStatusCode.NotFound, "Guarantor not found")]
    public async Task<ActionResult<GetGuarantorVm>> GetAsync([FromRoute] int id)
    {
        var result = await _mediator.Send(new GetGuarantorByIdQuery(id));
        return Ok(result);
    }


    [HttpGet]
    [SwaggerOperation("Get guarantor list")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Guarantors list returned", typeof(GetGuarantorsVm))]
    public async Task<ActionResult<GetGuarantorsVm>> GetListAsync([FromQuery] GetGuarantorsModel request)
    {
        var guarantors = await _mediator.Send(new GetGuarantorsQuery(request.TenantId, request.PageIndex, request.PageSize, request.SortColumn,
            request.SortDirection, request.SearchValue, request.PersonType));

        return Ok(guarantors);
    }
}