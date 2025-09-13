using Application.Command.WalletCommands;
using Application.Query.Queries;
using Application.Service.Dtos.Wallet;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.IdentityServerProvider.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers;

[ApiVersion("1.0")]
[Route("api/tenant-panel/wallets")]
[ApiController]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public WalletController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HttpGet("calculate/pre-payment/{walletId}/{amount}")]
    [SwaggerOperation("Calculate pre payment amount")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Pre payment amount returned", typeof(decimal))]
    public async Task<ActionResult<decimal>> CalculatePrePaymentAmount(int walletId, decimal amount)
    {
        var result = await _mediator.Send(new CalculatePrePaymentAmountQuery(walletId, amount));

        return Ok(result);
    }


    [HttpGet("pre-payment/details/{walletId}/{amount}")]
    [SwaggerOperation("Wallet pre payment details")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Wallet pre payment details returned", typeof(GetWalletPrePaymentDetails))]
    public async Task<ActionResult<GetWalletPrePaymentDetails>> GetWalletPrePaymentDetails(int walletId, decimal amount)
    {
        var result = await _mediator.Send(new GetWalletPrePaymentDetailsQuery(walletId, amount));
        return Ok(result);
    }

    [HttpPut("{id}/suspend")]
    [ActionName(nameof(SuspendAsync))]
    [SwaggerOperation("Suspend a wallet")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Suspended", typeof(void))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "Some validation or business error")]
    public async Task<ActionResult> SuspendAsync([FromRoute] int id)
    {
        await _mediator.Send(new SuspendWalletCommand(id, _currentUserService.TenantId));
        return Ok();
    }

    [HttpPut("{id}/activate")]
    [ActionName(nameof(ActivateAsync))]
    [SwaggerOperation("Activate a wallet")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Activated", typeof(void))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "Some validation or business error")]
    public async Task<ActionResult> ActivateAsync([FromRoute] int id)
    {
        await _mediator.Send(new ActivateWalletCommand(id, _currentUserService.TenantId));
        return Ok();
    }
}
