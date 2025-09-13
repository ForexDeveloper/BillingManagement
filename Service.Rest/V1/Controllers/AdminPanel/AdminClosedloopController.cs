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
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers.AdminPanel
{
    [ApiVersion("1.0")]
    [Route("api/admin-panel/closed-loops")]
    [ApiController]
    [Authorize]
    public class AdminClosedloopController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminClosedloopController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [SwaggerOperation("Create a new closed loop")]
        [SwaggerResponse((int)HttpStatusCode.Created, "Created", typeof(int))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
        public async Task<ActionResult<int>> Post(CreateClosedloopModel request)
        {
            var closedloopId = await _mediator.Send(new CreateClosedloopCommand(request.WalletConfigurationId, request.TenantId,
                request.Title, request.Categories, request.Merchants));
            return CreatedAtAction(nameof(GetAsync), new { id = closedloopId }, closedloopId);
        }

        [HttpPut]
        [Route("{id}")]
        [SwaggerOperation("Update a closed loop by id")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "closed loop info not found")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
        public async Task<ActionResult> Put(UpdateClosedloopModel request, [FromRoute] int id)
        {
            var closedloopId = await _mediator.Send(new UpdateClosedloopCommand(id, request.WalletConfigurationId, request.TenantId,
                 request.Title, request.Categories, request.Merchants));
            return Ok();
        }

        [HttpGet]
        [ActionName(nameof(GetAsync))]
        [Route("{id}")]
        [SwaggerOperation("get a closed loop by id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "closed loop returned", typeof(ClosedloopViewModel))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "closed loop not found")]
        public async Task<ActionResult<ClosedloopViewModel>> GetAsync([FromRoute] int id)
        {
            var result = await _mediator.Send(new GetClosedloopByIdQuery(id));
            return Ok(result);
        }

        [HttpGet]
        [SwaggerOperation("get closed loop list By tenant id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "closed loop list returned", typeof(GetClosedloopForGridViewModel))]
        public async Task<ActionResult<GetClosedloopForGridViewModel>> GetListAsync([FromQuery] GetClosedloopModel request)
        {
            var result = await _mediator.Send(new GetAllClosedloopQuery(request, request.TenantId, request.WalletConfigurationId));
            return Ok(result);
        }

        [HttpGet("wallet-configurations")]
        [SwaggerOperation("get wallet configuration list By tenant id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "wallet configuration list returned", typeof(GetWalletConfigurationWithOutCashWalletViewModel))]
        public async Task<ActionResult<List<GetWalletConfigurationWithOutCashWalletViewModel>>> GetListWalletConfigurationAsync([FromQuery] int? tenantId)
        {
            var result = await _mediator.Send(new GetAllWalletConfigurationWithOutCashWalletQuery(tenantId));
            return Ok(result);
        }
    }

}