using Application.Query.Queries.Merchants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.IdentityServerProvider.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers;

[ApiVersion("1.0")]
[Route("api/tenant-panel/merchant-branches")]
[ApiController]
[Authorize]
public class MerchantBranchController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public MerchantBranchController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HttpGet("{id:int}/active-contract-exists")]
    [SwaggerOperation("Has tenant merchant contract")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Has tenant merchant contractn returned", typeof(bool))]
    [SwaggerResponse((int)HttpStatusCode.NotFound, "Tenant merchant contractn not found")]
    public async Task<ActionResult<bool>> HasActiveTenantMerchantContract([FromRoute] int id)
    {
        return Ok(await _mediator.Send(new HasActiveTenantMerchantContractByMerchantBranchIdQuery(id, _currentUserService.TenantId)));
    }
}
