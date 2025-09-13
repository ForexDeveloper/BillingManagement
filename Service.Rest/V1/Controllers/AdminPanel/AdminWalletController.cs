using Application.Command.WalletCommands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Rest.V1.RequestModels.Wallets;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers.AdminPanel;

[ApiVersion("1.0")]
[Route("api/admin-panel/wallets")]
[ApiController]
[Authorize]
public class AdminWalletController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminWalletController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut("{id}/suspend")]
    [ActionName(nameof(SuspendAsync))]
    [SwaggerOperation("Suspend a wallet")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Suspended", typeof(void))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "Some validation or business error")]
    public async Task<ActionResult> SuspendAsync(ChangeWalletStatusModel request, [FromRoute] int id)
    {
        await _mediator.Send(new SuspendWalletCommand(id, request.TenantId));
        return Ok();
    }

    [HttpPut("{id}/activate")]
    [ActionName(nameof(ActivateAsync))]
    [SwaggerOperation("Activate a wallet")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Activated", typeof(void))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "Some validation or business error")]
    public async Task<ActionResult> ActivateAsync(ChangeWalletStatusModel request, [FromRoute] int id)
    {
        await _mediator.Send(new ActivateWalletCommand(id, request.TenantId));
        return Ok();
    }
}
