using Application.Command.TenantPlatformContractCommands;
using Application.Query.Queries;
using Application.Query.ViewModels.TenantPlatfromContracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Rest.V1.RequestModels.TenantPlatfromContracts;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.Net;

namespace Service.Rest.V1.Controllers.PlatformPanel
{
    [ApiVersion("1.0")]
    [Route("api/platform-panel/tenant-platform-contracts")]
    [ApiController]
    [Authorize]
    public class PlatformTenantPlatformContractController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PlatformTenantPlatformContractController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpPost]
        [ActionName(nameof(Create))]
        [SwaggerOperation("Create a new tenant platform contract")]
        [SwaggerResponse((int)HttpStatusCode.Created, "Created", typeof(int))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "Some validation or business error")]
        [SwaggerResponseHeader(201, "Location", "String", "Created wallet contract get url")]
        public async Task<ActionResult<int>> Create(CreateTenantPlatformContractModel request)
        {
            var contractId = await _mediator.Send(new CreateTenantPlatformContractCommand(
                    request.TenantId,
                    request.ContractNumber,
                    request.StartDate,
                    request.EndDate,
                    request.Description,
                    request.FeeCalculationType,
                    request.CommissionCalculationType,
                    request.TieredCommissions,
                    request.FixedAmount,
                    request.FixedAmountCommission,
                    request.FixedPercentageCommission,
                    request.CommissionReferenceTypes,
                    request.TransactionMinCommissionAmount,
                    request.TransactionMaxCommissionAmount,
                    request.PeriodMinCommissionAmount,
                    request.PeriodMaxCommissionAmount,
                    request.BillingPeriodType,
                    request.BillingPeriod,
                    request.DailyBillingOriginDate,
                    request.GracePeriod,
                    request.PenaltyPercent,
                    request.TenantIpgSettingId,
                    request.Facilitators,
                    request.Providers
                ));

            return CreatedAtAction(nameof(GetAsync), new { id = contractId }, contractId);
        }


        [HttpPut("{id}")]
        [ActionName(nameof(Update))]
        [SwaggerOperation("Update tenant platform contract")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Updated")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Wallet contract not found")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "Some validation or business error")]
        public async Task<ActionResult> Update([FromRoute] int id, UpdateTenantPlatformContractModel request)
        {
            var contractId = await _mediator.Send(new UpdateTenantPlatformContractCommand(
                    id,
                    request.TenantId,
                    request.ContractNumber,
                    request.StartDate,
                    request.EndDate,
                    request.Description,
                    request.FeeCalculationType,
                    request.CommissionCalculationType,
                    request.TieredCommissions,
                    request.FixedAmount,
                    request.FixedAmountCommission,
                    request.FixedPercentageCommission,
                    request.CommissionReferenceTypes,
                    request.TransactionMinCommissionAmount,
                    request.TransactionMaxCommissionAmount,
                    request.PeriodMinCommissionAmount,
                    request.PeriodMaxCommissionAmount,
                    request.BillingPeriodType,
                    request.BillingPeriod,
                    request.DailyBillingOriginDate,
                    request.GracePeriod,
                    request.PenaltyPercent,
                    request.TenantIpgSettingId,
                    request.Facilitators,
                    request.Providers
            ));

            return Ok();
        }


        [HttpGet("{id}")]
        [ActionName(nameof(GetAsync))]
        [SwaggerOperation("Get a tenant platform contract by id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Contract returned", typeof(GetTenantPlatformContractVm))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Contract not found")]
        public async Task<ActionResult<GetTenantPlatformContractVm>> GetAsync([FromRoute] int id)
        {
            var contractModel = await _mediator.Send(new GetTenantPlatformContractByIdQuery(id));
            return Ok(contractModel);
        }


        [HttpPost("{id}/clone")]
        [ActionName(nameof(CloneAsync))]
        [SwaggerOperation("Create a new tenant platform contract")]
        [SwaggerResponse((int)HttpStatusCode.Created, "Created", typeof(int))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
        [SwaggerResponseHeader(201, "Location", "String", "Created wallet contract get url")]
        public async Task<ActionResult<int>> CloneAsync([FromRoute] int id, CloneTenantPlatformContractModel request)
        {
            var contractId = await _mediator.Send(new CloneTenantPlatformContractCommand(
                id,
                    request.TenantId,
                    request.ContractNumber,
                    request.StartDate,
                    request.EndDate,
                    request.Description,
                    request.FeeCalculationType,
                    request.CommissionCalculationType,
                    request.TieredCommissions,
                    request.FixedAmount,
                    request.FixedAmountCommission,
                    request.FixedPercentageCommission,
                    request.CommissionReferenceTypes,
                    request.TransactionMinCommissionAmount,
                    request.TransactionMaxCommissionAmount,
                    request.PeriodMinCommissionAmount,
                    request.PeriodMaxCommissionAmount,
                    request.BillingPeriodType,
                    request.BillingPeriod,
                    request.DailyBillingOriginDate,
                    request.GracePeriod,
                    request.PenaltyPercent,
                    request.TenantIpgSettingId,
                    request.Facilitators,
                    request.Providers
            ));

            return CreatedAtAction(nameof(GetAsync), new { id = contractId }, contractId);
        }


        [HttpGet]
        [ActionName(nameof(GetListAsync))]
        [SwaggerOperation("Get tenant platform contracts list")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Tenant platform contracts list returned", typeof(GetTenantPlatformContractsVm))]
        public async Task<ActionResult<GetTenantPlatformContractsVm>> GetListAsync([FromQuery] GetTenantPlatformContractsModel request)
        {
            var contracts = await _mediator.Send(new GetTenantPlatformContractsQuery(
                request.ContractNumber,
                request.TenantId,
                request.StartDate,
                request.EndDate,
                request.FeeCalculationType,
                request.CommissionCalculationType,
                request.PageIndex,
                request.PageSize,
                request.SortColumn,
                request.SortDirection,
                request.SearchValue));

            return Ok(contracts);
        }


        [HttpPut("{id}/change-status")]
        [ActionName(nameof(ChangeStatus))]
        [SwaggerOperation("Tenant platform contract status updated")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Updated")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Tenant platform contract not found")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "Some validation or business error")]
        public async Task<ActionResult> ChangeStatus([FromRoute] int id, TenantPlatformContractStatusModel request)
        {
            var contractId = await _mediator.Send(new UpdateTenantPlatformContractStatusCommand(id, request.Status));

            return Ok();
        }

    }
}
