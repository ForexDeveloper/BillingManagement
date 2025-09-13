using Application.Command.WalletContractCommands;
using Application.Query.Queries;
using Application.Query.ViewModels.WalletContracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Rest.V1.RequestModels.TenantMerchantContracts;
using Service.Rest.V1.RequestModels.WalletContracts;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.Net;

namespace Service.Rest.V1.Controllers.AdminPanel
{
    [ApiVersion("1.0")]
    [Route("api/admin-panel/wallet-contracts")]
    [ApiController]
    [Authorize]
    public class AdminWalletContractController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminWalletContractController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpPost]
        [ActionName(nameof(Create))]
        [SwaggerOperation("Create a new wallet contract")]
        [SwaggerResponse((int)HttpStatusCode.Created, "Created", typeof(int))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
        [SwaggerResponseHeader(201, "Location", "String", "Created wallet contract get url")]
        public async Task<ActionResult<int>> Create(CreateWalletContractModel request)
        {
            var contractId = await _mediator.Send(new CreateWalletContractCommand(
                    request.TenantId,
                    request.PlanIds,
                    request.OrganizationId,
                    request.StartDate,
                    request.EndDate,
                    request.TenantIpgSettingId,
                    request.AssignWalletToOrganizationCustomers,
                    request.Guarantor,
                    request.Customers,
                    request.Financier,
                    request.Facilitators
                ));

            return CreatedAtAction(nameof(GetAsync), new { id = contractId }, contractId);
        }


        [HttpPut("{id}")]
        [ActionName(nameof(Update))]
        [SwaggerOperation("Update wallet contract")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Updated")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "wallet contract not found")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
        public async Task<ActionResult> Update(UpdateWalletContractModel request, [FromRoute] int id)
        {
            var contractId = await _mediator.Send(new UpdateWalletContractCommand(
                id,
                request.TenantId,
                request.PlanIds,
                request.OrganizationId,
                request.StartDate,
                request.EndDate,
                request.TenantIpgSettingId,
                request.AssignWalletToOrganizationCustomers,
                request.Guarantor,
                request.Customers,
                request.Financier,
                request.Facilitators
            ));

            return Ok();
        }


        [HttpPost("{id}/clone")]
        [ActionName(nameof(CloneAsync))]
        [SwaggerOperation("Create a new wallet contract")]
        [SwaggerResponse((int)HttpStatusCode.Created, "Created", typeof(int))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "Some validation or business error")]
        [SwaggerResponseHeader(201, "Location", "String", "Created wallet contract get url")]
        public async Task<ActionResult<int>> CloneAsync([FromRoute] int id, CloneWalletContractModel request)
        {
            var contractId = await _mediator.Send(new CloneWalletContractCommand(
                   id,
                    request.TenantId,
                    request.StartDate,
                    request.EndDate,
                    request.TenantIpgSettingId,
                    request.AssignWalletToOrganizationCustomers,
                    request.Guarantor,
                    request.Customers,
                    request.Financier,
                    request.Facilitators
                ));

            return CreatedAtAction(nameof(GetAsync), new { id = contractId }, contractId);
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetAsync))]
        [SwaggerOperation("Get wallet contract by id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Contract returned", typeof(GetWalletContractVm))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Contract not found")]
        public async Task<ActionResult<GetWalletContractVm>> GetAsync([FromRoute] int id)
        {
            var contract = await _mediator.Send(new GetWalletContractByIdQuery(id));
            return Ok(contract);
        }


        [HttpGet]
        [ActionName(nameof(GetListAsync))]
        [SwaggerOperation("Get wallet contracts list")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Wallet contracts list returned", typeof(GetWalletContractsVm))]
        public async Task<ActionResult<GetWalletContractsVm>> GetListAsync([FromQuery] GetWalletContractsModel request)
        {
            var contracts = await _mediator.Send(new GetWalletContractsQuery(
                request.TenantId,
                request.OrganizationIds,
                request.Status,
                request.EndDate,
                request.PageIndex,
                request.PageSize,
                request.SortColumn,
                request.SortDirection,
                request.SearchValue));

            return Ok(contracts);
        }


        [HttpGet("{id}/endorsements")]
        [ActionName(nameof(GetEndorsementListAsync))]
        [SwaggerOperation("Get endorsements list")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Wallet contract endorsements list returned", typeof(List<WalletContractEndorsementVm>))]
        public async Task<ActionResult<List<WalletContractEndorsementVm>>> GetEndorsementListAsync([FromRoute] int id)
        {
            var endorsements = await _mediator.Send(new GetWalletContractEndorsementsByRootParentIdQuery(id));

            return Ok(endorsements);
        }


        [HttpGet("{id}/customers")]
        [ActionName(nameof(GetCustomerListAsync))]
        [SwaggerOperation("Get wallet contracts customers list")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Wallet contracts customers list returned", typeof(GetWalletContractCustomersVm))]
        public async Task<ActionResult<GetWalletContractCustomersVm>> GetCustomerListAsync([FromRoute] int id, [FromQuery] GetWalletContractsCustomerModel request)
        {
            var contracts = await _mediator.Send(new GetWalletContractsCustomerQuery(
                id,
                request.PageIndex,
                request.PageSize,
                request.SortColumn,
                request.SortDirection,
                request.SearchValue));

            return Ok(contracts);
        }


        [HttpPut("{id}/change-status")]
        [ActionName(nameof(ChangeActivation))]
        [SwaggerOperation("Wallet contract activation status updated")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Updated")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "wallet contract not found")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
        public async Task<ActionResult> ChangeActivation(WalletContractActivationModel request, [FromRoute] int id)
        {
            var contractId = await _mediator.Send(new UpdateWalletContractActivationStatusCommand(id, request.Status));

            return Ok();
        }


        [HttpPut("{id}/acceptance")]
        [ActionName(nameof(ChangeAcceptance))]
        [SwaggerOperation("Wallet contract acceptance status updated")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Updated")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "wallet contract not found")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
        public async Task<ActionResult> ChangeAcceptance(WalletContractAcceptanceModel request, [FromRoute] int id)
        {
            var contractId = await _mediator.Send(new UpdateWalletContractAcceptanceStatusCommand(id, request.Status, request.Reason));

            return Ok();
        }


        [HttpGet("{id}/rejections")]
        [ActionName(nameof(GetCustomerListAsync))]
        [SwaggerOperation("Get wallet contracts rejections list")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Wallet contracts rejections list returned", typeof(List<WalletContractRejectionVm>))]
        public async Task<ActionResult<List<WalletContractRejectionVm>>> GetRejectionsListAsync([FromRoute] int id)
        {
            var contracts = await _mediator.Send(new GetWalletContractsRejectionQuery(id));

            return Ok(contracts);
        }
    }
}
