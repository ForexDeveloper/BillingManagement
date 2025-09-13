using Application.Command.WalletContractCommands;
using Application.Query.Queries;
using Application.Query.ViewModels.WalletContracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Rest.V1.RequestModels.TenantMerchantContracts;
using Service.Rest.V1.RequestModels.WalletContracts;
using Shared.IdentityServerProvider.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.Net;

namespace Service.Rest.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/tenant-panel/wallet-contracts")]
    [ApiController]
    [Authorize]
    public class WalletContractController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;

        public WalletContractController(IMediator mediator, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
        }


        [HttpPost]
        [ActionName(nameof(Create))]
        [SwaggerOperation("Create a new wallet contract")]
        [SwaggerResponse((int)HttpStatusCode.Created, "Created", typeof(int))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
        [SwaggerResponseHeader(201, "Location", "String", "Created wallet contract get url")]
        public async Task<ActionResult<int>> Create(CreateWalletContractBaseModel request)
        {
            var contractId = await _mediator.Send(new CreateWalletContractCommand(
                _currentUserService.TenantId,
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
        public async Task<ActionResult> Update(UpdateWalletContractBaseModel request, [FromRoute] int id)
        {
            var contractId = await _mediator.Send(new UpdateWalletContractCommand(
                id,
                _currentUserService.TenantId,
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
        public async Task<ActionResult<int>> CloneAsync([FromRoute] int id, CloneWalletContractBaseModel request)
        {
            var contractId = await _mediator.Send(new CloneWalletContractCommand(
                   id,
                    _currentUserService.TenantId,
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
            var contractModel = await _mediator.Send(new GetWalletContractByIdQuery(id, _currentUserService.TenantId));
            return Ok(contractModel);
        }


        [HttpGet]
        [ActionName(nameof(GetListAsync))]
        [SwaggerOperation("Get wallet contracts list")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Wallet contracts list returned", typeof(GetWalletContractsVm))]
        public async Task<ActionResult<GetWalletContractsVm>> GetListAsync([FromQuery] GetTenantWalletContractsModel request)
        {
            var contracts = await _mediator.Send(new GetWalletContractsQuery(
                _currentUserService.TenantId,
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
            var endorsements = await _mediator.Send(new GetWalletContractEndorsementsByRootParentIdQuery(id, _currentUserService.TenantId));

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
                request.SearchValue,
                _currentUserService.TenantId));

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
            var contractId = await _mediator.Send(new UpdateWalletContractActivationStatusCommand(id, request.Status, _currentUserService.TenantId));

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
            var contractId = await _mediator.Send(new UpdateWalletContractAcceptanceStatusCommand(id, request.Status, request.Reason, _currentUserService.TenantId));

            return Ok();
        }


        [HttpGet("{id}/rejections")]
        [ActionName(nameof(GetCustomerListAsync))]
        [SwaggerOperation("Get wallet contracts rejections list")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Wallet contracts rejections list returned", typeof(List<WalletContractRejectionVm>))]
        public async Task<ActionResult<List<WalletContractRejectionVm>>> GetRejectionsListAsync([FromRoute] int id)
        {
            var contracts = await _mediator.Send(new GetWalletContractsRejectionQuery(id, _currentUserService.TenantId));

            return Ok(contracts);
        }

    }
}
