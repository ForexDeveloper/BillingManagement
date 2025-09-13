using Application.Command.ClosedloopCommands;
using Application.Query.Queries;
using Application.Query.QueryModels;
using Application.Query.ViewModels.Closedloops;
using Application.Query.ViewModels.WalletConfigurations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Rest.V1.RequestModels.Closedloops;
using Service.Rest.V1.RequestModels.WalletConfigurations;
using Shared.IdentityServerProvider.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/tenant-panel/closed-loops")]
    [ApiController]
    [Authorize]
    public class ClosedloopController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;

        public ClosedloopController(IMediator mediator, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
        }

        [HttpPost]
        [SwaggerOperation("Create a new close loop")]
        [SwaggerResponse((int)HttpStatusCode.Created, "Created", typeof(int))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
        public async Task<ActionResult<int>> Post(CreateClosedloopBaseModel request)
        {
            var closedloopId = await _mediator.Send(new CreateClosedloopCommand(request.WalletConfigurationId, _currentUserService.TenantId,
                request.Title, request.Categories, request.Merchants));
            return CreatedAtAction(nameof(GetAsync), new { id = closedloopId }, closedloopId);
        }

        [HttpPut]
        [Route("{id}")]
        [SwaggerOperation("Update a close loop by id")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "close loop info not found")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
        public async Task<ActionResult> Put(UpdateClosedloopBaseModel request, [FromRoute] int id)
        {
            var closedloopId = await _mediator.Send(new UpdateClosedloopCommand(id, request.WalletConfigurationId, _currentUserService.TenantId,
                 request.Title, request.Categories, request.Merchants));
            return Ok();
        }

        [HttpGet]
        [ActionName(nameof(GetAsync))]
        [Route("{id}")]
        [SwaggerOperation("get a close loop by id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "close loop returned", typeof(ClosedloopViewModel))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "close loop not found")]
        public async Task<ActionResult<ClosedloopViewModel>> GetAsync([FromRoute] int id)
        {
            var result = await _mediator.Send(new GetClosedloopByIdQuery(id, _currentUserService.TenantId));
            return Ok(result);
        }

        [HttpGet]
        [SwaggerOperation("get close loop list By tenant id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "close loop list returned", typeof(GetClosedloopForGridViewModel))]
        public async Task<ActionResult<GetClosedloopForGridViewModel>> GetListAsync([FromQuery] GetClosedloopBaseModel request)
        {
            var result = await _mediator.Send(new GetAllClosedloopQuery(request, _currentUserService.TenantId, request.WalletConfigurationId));
            return Ok(result);
        }
        [HttpGet("wallet-configurations")]
        [SwaggerOperation("get wallet configuration list By tenant id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "wallet configuration list returned", typeof(GetWalletConfigurationWithOutCashWalletViewModel))]
        public async Task<ActionResult<List<GetWalletConfigurationWithOutCashWalletViewModel>>> GetListWalletConfigurationAsync()
        {
            var result = await _mediator.Send(new GetAllWalletConfigurationWithOutCashWalletQuery(_currentUserService.TenantId));
            return Ok(result);
        }
    }

}