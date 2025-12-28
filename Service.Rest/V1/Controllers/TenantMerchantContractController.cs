using MediatR;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using Shared.IdentityServerProvider.Contracts;
using Application.Query.Queries.TenantMerchantContracts;
using Application.Command.TenantMerchantContractCommands;
using Application.Query.ViewModels.TenantMerchantContracts;
using Service.Rest.V1.RequestModels.TenantMerchantContracts;

namespace Service.Rest.V1.Controllers;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/tenant-panel/tenant-merchant-contracts")]
public class TenantMerchantContractController(IMediator mediator, ICurrentUserService currentUserService)
    : ControllerBase
{
    [HttpPost]
    [ActionName(nameof(Create))]
    [SwaggerOperation("Create a new tenant merchant contract")]
    [SwaggerResponse((int)HttpStatusCode.Created, "Created", typeof(int))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
    [SwaggerResponseHeader(201, "Location", "String", "Created wallet contract get url")]
    public async Task<ActionResult<int>> Create(CreateTenantMerchantContractBaseModel request)
    {
        var contractId = await mediator.Send(new CreateTenantMerchantContractCommand(
           currentUserService.TenantId,
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
    [SwaggerResponse((int)HttpStatusCode.OK, "Updated", typeof(int))]
    [SwaggerResponse((int)HttpStatusCode.NotFound, "Tenant merchant contract not found")]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
    public async Task<ActionResult> Update([FromRoute] int id, UpdateTenantMerchantContractBaseModel request)
    {
        var contractId = await mediator.Send(new UpdateTenantMerchantContractCommand(
            id,
            currentUserService.TenantId,
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
        var contractModel = await mediator.Send(new GetTenantMerchantContractByIdQuery(id, currentUserService.TenantId));
        return Ok(contractModel);
    }


    [HttpGet]
    [ActionName(nameof(GetListAsync))]
    [SwaggerOperation("Get tenant merchant contracts list")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Contracts list returned", typeof(GetTenantMerchantContractsVm))]
    public async Task<ActionResult<GetTenantMerchantContractsVm>> GetListAsync([FromQuery] GetTenantMerchantContractsBaseModel request)
    {
        var tenants = await mediator.Send(new GetTenantMerchantContractsQuery(
            currentUserService.TenantId,
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
        var contractModel = await mediator.Send(new GetTenantMerchantContractAttachmentsByContactIdQuery(id, currentUserService.TenantId));
        return Ok(contractModel);
    }


    [HttpPut("{id}/change-status")]
    [ActionName(nameof(ChangeStatus))]
    [SwaggerOperation("Tenant merchant contract status updated")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Updated")]
    [SwaggerResponse((int)HttpStatusCode.NotFound, "Tenant merchant contract not found")]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
    public async Task<ActionResult> ChangeStatus([FromRoute] int id, TenantMerchantContractStatusBaseModel request)
    {
        var contractId = await mediator.Send(new UpdateTenantMerchantContractStatusCommand(id, request.Status, currentUserService.TenantId));

        return Ok();
    }


    [HttpPost("{id}/clone")]
    [ActionName(nameof(CloneAsync))]
    [SwaggerOperation("Create a new tenant merchant contract")]
    [SwaggerResponse((int)HttpStatusCode.Created, "Created", typeof(int))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
    [SwaggerResponseHeader(201, "Location", "String", "Created tenant merchant contract get url")]
    public async Task<ActionResult<int>> CloneAsync([FromRoute] int id, CloneTenantMerchantContractBaseModel request)
    {
        var contractId = await mediator.Send(new CloneTenantMerchantContractCommand(
            id,
            currentUserService.TenantId,
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