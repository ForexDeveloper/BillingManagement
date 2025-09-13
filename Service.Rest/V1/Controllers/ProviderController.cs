using Application.Query.Queries;
using Application.Query.ViewModels.Providers;
using Domain.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.IdentityServerProvider.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers;

[ApiVersion("1.0")]
[Route("api/tenant-panel/providers")]
[ApiController]
[Authorize]
public class ProviderController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    public ProviderController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [ActionName(nameof(GetByTenantIdAsync))]
    [SwaggerOperation("Get an provider by tenant-id")]
    [SwaggerResponse((int)HttpStatusCode.OK, "provider returned", typeof(GetProviderVm))]
    [SwaggerResponse((int)HttpStatusCode.NotFound, "provider not found")]
    public async Task<ActionResult> GetByTenantIdAsync([FromQuery] ProviderType? type)
    {
        var result = await _mediator.Send(new GetProvidersByTenantIdQuery(type, _currentUserService.TenantId));
        return Ok(result);
    }
}
