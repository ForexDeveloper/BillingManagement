using Application.Command.TenantMerchantContractCommands;
using Application.Query.Queries.TenantMerchantContracts;
using Application.Query.ViewModels.TenantMerchantContracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Rest.V1.RequestModels.TenantMerchantContracts;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.Net;

namespace Service.Rest.V1.Controllers.AdminPanel
{
    [ApiVersion("1.0")]
    [Route("api/admin-panel/tenant-merchant-contracts")]
    [ApiController]
    [Authorize]
    public class AdminTenantMerchantContractController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminTenantMerchantContractController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpPost]
        [ActionName(nameof(Create))]
        [SwaggerOperation("Create a new tenant merchant contract")]
        [SwaggerResponse((int)HttpStatusCode.Created, "Created", typeof(int))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "Some validation or business error")]
        [SwaggerResponseHeader(201, "Location", "String", "Created tenant merchant contract get url")]
        public async Task<ActionResult<int>> Create(CreateTenantMerchantContractModel request)
        {
            var contractId = await _mediator.Send(new CreateTenantMerchantContractCommand(
                request.TenantId,
                request.MerchantId,
                request.ContractDocument,
                request.ContractNumber,
                request.StartDate,
                request.EndDate,
                request.SettlementType,
                request.IsCommissionExchanged,
                request.InstallmentsCount,
                request.CommissionDeductionMethodType,
                request.InterestPercentage,
                request.InterestReferenceTypes,
                request.BillingPeriodType,
                request.BillingPeriod,
                request.DailyBillingOriginDate,
                request.BillingBreak,
                request.PaymentMethodType,
                request.GuaranteeType,
                request.GuaranteeDescription,
                request.CommissionCalculationType,
                request.TieredCommissions,
                request.FixedAmountCommission,
                request.FixedPercentageCommission,
                request.CommissionReferenceTypes,
                request.TransactionMinCommissionAmount,
                request.TransactionMaxCommissionAmount,
                request.PeriodMinCommissionAmount,
                request.PeriodMaxCommissionAmount
                ));

            return CreatedAtAction(nameof(GetAsync), new { id = contractId }, contractId);
        }


        [HttpPut("{id}")]
        [ActionName(nameof(Update))]
        [SwaggerOperation("Update tenant merchant contract")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Updated")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Tenant merchant contract not found")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
        public async Task<ActionResult> Update([FromRoute] int id, UpdateTenantMerchantContractModel request)
        {
            var contractId = await _mediator.Send(new UpdateTenantMerchantContractCommand(
                id,
                request.TenantId,
                request.MerchantId,
                request.ContractDocument,
                request.ContractNumber,
                request.StartDate,
                request.EndDate,
                request.SettlementType,
                request.IsCommissionExchanged,
                request.InstallmentsCount,
                request.CommissionDeductionMethodType,
                request.InterestPercentage,
                request.InterestReferenceTypes,
                request.BillingPeriodType,
                request.BillingPeriod,
                request.DailyBillingOriginDate,
                request.BillingBreak,
                request.PaymentMethodType,
                request.GuaranteeType,
                request.GuaranteeDescription,
                request.CommissionCalculationType,
                request.TieredCommissions,
                request.FixedAmountCommission,
                request.FixedPercentageCommission,
                request.CommissionReferenceTypes,
                request.TransactionMinCommissionAmount,
                request.TransactionMaxCommissionAmount,
                request.PeriodMinCommissionAmount,
                request.PeriodMaxCommissionAmount
            ));

            return Ok();
        }


        [HttpGet("{id}")]
        [ActionName(nameof(GetAsync))]
        [SwaggerOperation("Get tenant merchant contract by id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Contract returned", typeof(GetTenantMerchantContractVm))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Contract not found")]
        public async Task<ActionResult<GetTenantMerchantContractVm>> GetAsync([FromRoute] int id)
        {
            var contractModel = await _mediator.Send(new GetTenantMerchantContractByIdQuery(id));
            return Ok(contractModel);
        }


        [HttpGet]
        [ActionName(nameof(GetListAsync))]
        [SwaggerOperation("Get tenant merchant contracts list")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Contracts list returned", typeof(GetTenantMerchantContractsVm))]
        public async Task<ActionResult<GetTenantMerchantContractsVm>> GetListAsync([FromQuery] GetTenantMerchantContractsModel request)
        {
            var tenants = await _mediator.Send(new GetTenantMerchantContractsQuery(
                request.TenantId,
                request.MerchantId,
                request.GuaranteeType,
                request.SettlementType,
                request.PaymentMethodType,
                request.PageIndex,
                request.PageSize,
                request.SortColumn,
                request.SortDirection,
                request.SearchValue));

            return Ok(tenants);
        }


        [HttpGet("{id}/documents")]
        [ActionName(nameof(GetAttachmentListAsync))]
        [SwaggerOperation("Get tenant merchant contract documents by contract id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Contract documents returned", typeof(List<GetTenantMerchantDocumentVm>))]
        public async Task<ActionResult<List<GetTenantMerchantDocumentVm>>> GetAttachmentListAsync([FromRoute] int id)
        {
            var contractModel = await _mediator.Send(new GetTenantMerchantContractAttachmentsByContactIdQuery(id));

            return Ok(contractModel);
        }


        [HttpPut("{id}/change-status")]
        [ActionName(nameof(ChangeStatus))]
        [SwaggerOperation("Tenant merchant contract status updated")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Updated")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Tenant merchant contract not found")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
        public async Task<ActionResult> ChangeStatus([FromRoute] int id, TenantMerchantContractStatusModel request)
        {
            var contractId = await _mediator.Send(new UpdateTenantMerchantContractStatusCommand(id, request.Status, request.TenantId));

            return Ok();
        }


        [HttpPost("{id}/clone")]
        [ActionName(nameof(CloneAsync))]
        [SwaggerOperation("Create a new tenant merchant contract")]
        [SwaggerResponse((int)HttpStatusCode.Created, "Created", typeof(int))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
        [SwaggerResponseHeader(201, "Location", "String", "Created tenant merchant contract get url")]
        public async Task<ActionResult<int>> CloneAsync([FromRoute] int id, CloneTenantMerchantContractModel request)
        {
            var contractId = await _mediator.Send(new CloneTenantMerchantContractCommand(
                id,
                request.TenantId,
                request.MerchantId,
                request.ContractDocument,
                request.ContractNumber,
                request.StartDate,
                request.EndDate,
                request.SettlementType,
                request.IsCommissionExchanged,
                request.InstallmentsCount,
                request.CommissionDeductionMethodType,
                request.InterestPercentage,
                request.InterestReferenceTypes,
                request.BillingPeriodType,
                request.BillingPeriod,
                request.DailyBillingOriginDate,
                request.BillingBreak,
                request.PaymentMethodType,
                request.GuaranteeType,
                request.GuaranteeDescription,
                request.CommissionCalculationType,
                request.TieredCommissions,
                request.FixedAmountCommission,
                request.FixedPercentageCommission,
                request.CommissionReferenceTypes,
                request.TransactionMinCommissionAmount,
                request.TransactionMaxCommissionAmount,
                request.PeriodMinCommissionAmount,
                request.PeriodMaxCommissionAmount
            ));

            return CreatedAtAction(nameof(GetAsync), new { id = contractId }, contractId);
        }
    }
}
