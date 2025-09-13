using Application.Command.WalletConfigurationCommands;
using Application.Query.Queries;
using Application.Query.QueryModels;
using Application.Query.ViewModels.WalletConfigurations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Rest.V1.RequestModels.WalletConfigurations;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers.AdminPanel
{
    [ApiVersion("1.0")]
    [Route("api/admin-panel/wallet-configurations")]
    [ApiController]
    [Authorize]
    public class AdminWalletConfigurationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminWalletConfigurationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [SwaggerOperation("Create a new wallet configuration")]
        [SwaggerResponse((int)HttpStatusCode.Created, "Created", typeof(int))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
        public async Task<ActionResult<int>> Post(CreateWalletConfigurationModel request)
        {
            var createdWalletConfigurationId = await _mediator.Send(new CreateWalletConfigurationCommand(
             request.Title, request.WalletTypeId, request.MaxWallet, request.MaximumTotalCredit, request.TenantId, request.ProjectManagerId,
             request.CurrencyTypeId, request.Installments, request.Financial));
            return CreatedAtAction(nameof(GetAsync), new { id = createdWalletConfigurationId }, createdWalletConfigurationId);
        }

        [HttpPut]
        [Route("{id}")]
        [SwaggerOperation("Update a wallet configuration by id")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "wallet configuration info not found")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
        public async Task<ActionResult> Put(UpdateWalletConfigurationModel request, [FromRoute] int id)
        {
            var updateWalletConfigurationId = await _mediator.Send(new UpdateWalletConfigurationCommand(id, request.Title, request.WalletTypeId,
             request.MaxWallet, request.MaximumTotalCredit, request.ProjectManagerId,
             request.CurrencyTypeId, request.Installments,
             request.Financial));
            return Ok();
        }

        [HttpPatch("{id}")]
        [SwaggerOperation("Update a max wallet by id")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "wallet configuration info not found")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
        public async Task<ActionResult> UpdateMaxWallet([FromBody] UpdateMaxWalletModel model, [FromRoute] int id)
        {
            await _mediator.Send(new UpdateMaxWalletCommand(id, model.MaxWallet));
            return Ok();
        }

        [HttpGet]
        [ActionName(nameof(GetAsync))]
        [Route("{id}")]
        [SwaggerOperation("get a wallet configuration by id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "wallet configuration returned", typeof(WalletConfigurationViewModel))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "wallet configuration not found")]
        public async Task<ActionResult<WalletConfigurationViewModel>> GetAsync([FromRoute] int id)
        {
            var WalletConfigurationModel = await _mediator.Send(new GetWalletConfigurationByIdQuery(id));
            return Ok(WalletConfigurationModel);
        }

        [HttpGet]
        [SwaggerOperation("get wallet configuration list By tenant id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "wallet configuration list returned", typeof(GetWalletConfigurationQueryModel))]
        public async Task<ActionResult<GetWalletConfigurationViewModel>> GetListAsync([FromQuery] GetWalletConfigurationModel request)
        {
            var result = await _mediator.Send(new GetAllWalletConfigurationQuery(request, request.TenantId));
            return Ok(result);
        }
               
    }

}