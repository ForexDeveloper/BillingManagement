using MediatR;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;
using Application.Command.BillingCommands;
using Application.Query.ViewModels.Billings;
using Service.Rest.V1.RequestModels.Billings;
using Shared.IdentityServerProvider.Contracts;
using Application.Query.Queries.MerchantBilling;
using Application.Query.ViewModels.MerchantBillings;

namespace Service.Rest.V1.Controllers;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/tenant-panel/merchant-billings/{id}")]
public sealed class MerchantBillingController(IMediator mediator, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    [Route("/api/tenant-panel/merchants/{id}/billings")]
    [SwaggerOperation("Get merchant billing list")]
    [SwaggerResponse((int)HttpStatusCode.NotFound, "merchant billing list not found")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing list returned", typeof(GetBillingsVm))]
    public async Task<ActionResult> GetListAsync([FromRoute] int id, [FromQuery] GetBillingsRequest request)
    {
        var billings = await mediator.Send(new GetMerchantBillingsQuery(currentUserService.TenantId, id, request.Code,
            request.Type, request.Status, request.StartDate, request.DueDate, request.PageSize, request.PageIndex));

        return Ok(billings);
    }


    [HttpGet]
    [SwaggerOperation("Get merchant billing")]
    [SwaggerResponse((int)HttpStatusCode.NotFound, "merchant billing not found")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing returned", typeof(GetMerchantBillingVm))]
    public async Task<ActionResult> GetAsync([FromRoute] long id)
    {
        var billing = await mediator.Send(new GetMerchantBillingQuery(currentUserService.TenantId, id));

        return Ok(billing);
    }


    [HttpGet("additions")]
    [SwaggerOperation("Get merchant billing additions")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing additions returned", typeof(GetAdditionsVm))]
    public async Task<ActionResult> GetAdditions([FromRoute] long id)
    {
        var additions = await mediator.Send(new GetAdditionsQuery(currentUserService.TenantId, id));

        return Ok(additions);
    }


    [HttpPut("additions")]
    [SwaggerOperation("Update merchant billing additions")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing additions updated", typeof(long))]
    public async Task<ActionResult> UpdateAdditions([FromRoute] long id, UpdateBillingAdditionsRequest request)
    {
        var additions = await mediator.Send(new UpdateBillingAdditionsCommand(currentUserService.TenantId, id, request.Amount, request.Description));

        return Ok(additions);
    }


    [HttpGet("deductions")]
    [SwaggerOperation("Get merchant billing deductions")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing deductions returned", typeof(GetDeductionsVm))]
    public async Task<ActionResult> GetDeductions([FromRoute] long id)
    {
        var additions = await mediator.Send(new GetDeductionsQuery(currentUserService.TenantId, id));

        return Ok(additions);
    }


    [HttpPut("deductions")]
    [SwaggerOperation("Update merchant billing deductions")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing deductions updated", typeof(long))]
    public async Task<ActionResult> UpdateDeductions([FromRoute] long id, UpdateBillingDeductionsRequest request)
    {
        var additions = await mediator.Send(new UpdateBillingDeductionsCommand(currentUserService.TenantId, id, request.Amount, request.Description));

        return Ok(additions);
    }


    [HttpGet("previous-debit")]
    [SwaggerOperation("Get merchant billing previous debit")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing previous debit returned", typeof(GetPreviousDebitVm))]
    public async Task<ActionResult> GetPreviousDebit([FromRoute] long id)
    {
        var previousDebit = await mediator.Send(new GetPreviousDebitQuery(currentUserService.TenantId, id));

        return Ok(previousDebit);
    }


    [HttpGet("previous-credit")]
    [SwaggerOperation("Get merchant billing previous credit")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing previous credit returned", typeof(GetPreviousCreditVm))]
    public async Task<ActionResult> GetPreviousCredit([FromRoute] long id)
    {
        var previousCredit = await mediator.Send(new GetPreviousCreditQuery(currentUserService.TenantId, id));

        return Ok(previousCredit);
    }


    [HttpGet("purchase-transactions")]
    [SwaggerOperation("Get merchant billing purchase transactions")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing purchase transactions returned", typeof(GetPurchaseTransactionsVm))]
    public async Task<ActionResult> GetPurchaseTransactions([FromRoute] long id)
    {
        var transactions = await mediator.Send(new GetPurchaseTransactionsQuery(currentUserService.TenantId, id));

        return Ok(transactions);
    }


    [HttpGet("refunded-transactions")]
    [SwaggerOperation("Get merchant billing refunded transactions")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing refunded transactions returned", typeof(GetRefundedTransactionsVm))]
    public async Task<ActionResult> GetRefundedTransactions([FromRoute] long id)
    {
        var transactions = await mediator.Send(new GetRefundedTransactionsQuery(currentUserService.TenantId, id));

        return Ok(transactions);
    }


    [HttpGet("purchase-transactions-commission")]
    [SwaggerOperation("Get merchant billing purchase transactions commission")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing purchase transactions commission returned", typeof(GetPurchaseTransactionsCommissionVm))]
    public async Task<ActionResult> GetPurchaseTransactionsCommission([FromRoute] long id)
    {
        var commission = await mediator.Send(new GetPurchaseTransactionsCommissionQuery(currentUserService.TenantId, id));

        return Ok(commission);
    }


    [HttpGet("refunded-transactions-commission")]
    [SwaggerOperation("Get merchant billing refunded transactions commission")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing refunded transactions commission returned", typeof(GetRefundedTransactionsCommissionVm))]
    public async Task<ActionResult> GetRefundedTransactionsCommission([FromRoute] long id)
    {
        var commission = await mediator.Send(new GetRefundedTransactionsCommissionQuery(currentUserService.TenantId, id));

        return Ok(commission);
    }


    [HttpGet("pay-amount/{amount}/payable-amount")]
    [SwaggerOperation("Get billing payable amount")]
    [SwaggerResponse((int)HttpStatusCode.NotFound, "Merchant billing not found")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Merchant billing payable amount returned", typeof(decimal))]
    public async Task<ActionResult<decimal>> GetBillingPayableAmount([FromRoute] long id, [FromRoute] decimal amount)
    {
        var merchantBillingPayableAmount = await mediator.Send(new GetMerchantBillingPayableAmountQuery(currentUserService.TenantId, id, amount));

        return Ok(merchantBillingPayableAmount);
    }
}