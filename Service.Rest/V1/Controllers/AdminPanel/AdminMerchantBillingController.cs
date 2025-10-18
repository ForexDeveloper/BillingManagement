using MediatR;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;
using Application.Command.BillingCommands;
using Service.Rest.V1.RequestModels.Billings;
using Application.Query.Queries.MerchantBilling;
using Application.Query.ViewModels.MerchantBillings;

namespace Service.Rest.V1.Controllers.AdminPanel;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/admin-panel/merchant-billings")]
public sealed class AdminMerchantBillingController(IMediator mediator) : ControllerBase
{
    [HttpGet("{billingId}/tenants/{tenantId}")]
    [SwaggerOperation("Get merchant billing list")]
    [SwaggerResponse((int)HttpStatusCode.NotFound, "merchant billing not found")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing returned", typeof(GetMerchantBillingVm))]
    public async Task<ActionResult> GetAsync([FromRoute] int tenantId, [FromRoute] long billingId)
    {
        var billing = await mediator.Send(new GetMerchantBillingQuery(tenantId, billingId));

        return Ok(billing);
    }


    [HttpGet("{billingId}/tenants/{tenantId}/additions")]
    [SwaggerOperation("Get merchant billing additions")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing additions returned", typeof(GetAdditionsVm))]
    public async Task<ActionResult> GetAdditions([FromRoute] int tenantId, [FromRoute] long billingId)
    {
        var additions = await mediator.Send(new GetAdditionsQuery(tenantId, billingId));

        return Ok(additions);
    }


    [HttpPut("{billingId}/tenants/{tenantId}/additions")]
    [SwaggerOperation("Update merchant billing additions")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing additions updated", typeof(long))]
    public async Task<ActionResult> UpdateAdditions([FromRoute] int tenantId, [FromRoute] long billingId, UpdateBillingAdditionsRequest request)
    {
        var additions = await mediator.Send(new UpdateBillingAdditionsCommand(tenantId, billingId, request.Amount, request.Description));

        return Ok(additions);
    }


    [HttpGet("{billingId}/tenants/{tenantId}/deductions")]
    [SwaggerOperation("Get merchant billing deductions")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing deductions returned", typeof(GetDeductionsVm))]
    public async Task<ActionResult> GetDeductions([FromRoute] int tenantId, [FromRoute] long billingId)
    {
        var additions = await mediator.Send(new GetDeductionsQuery(tenantId, billingId));

        return Ok(additions);
    }


    [HttpPut("{billingId}/tenants/{tenantId}/deductions")]
    [SwaggerOperation("Update merchant billing deductions")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing deductions updated", typeof(long))]
    public async Task<ActionResult> UpdateDeductions([FromRoute] int tenantId, [FromRoute] long billingId, UpdateBillingDeductionsRequest request)
    {
        var deductions = await mediator.Send(new UpdateBillingDeductionsCommand(tenantId, billingId, request.Amount, request.Description));

        return Ok(deductions);
    }


    [HttpGet("{billingId}/tenants/{tenantId}/previous-debit")]
    [SwaggerOperation("Get merchant billing previous debit")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing previous debit returned", typeof(GetPreviousDebitVm))]
    public async Task<ActionResult> GetPreviousDebit([FromRoute] int tenantId, [FromRoute] long billingId)
    {
        var previousDebit = await mediator.Send(new GetPreviousDebitQuery(tenantId, billingId));

        return Ok(previousDebit);
    }


    [HttpGet("{billingId}/tenants/{tenantId}/previous-credit")]
    [SwaggerOperation("Get merchant billing previous debit")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing previous credit returned", typeof(GetPreviousCreditVm))]
    public async Task<ActionResult> GetPreviousCredit([FromRoute] int tenantId, [FromRoute] long billingId)
    {
        var previousCredit = await mediator.Send(new GetPreviousCreditQuery(tenantId, billingId));

        return Ok(previousCredit);
    }


    [HttpGet("{billingId}/tenants/{tenantId}/purchase-transactions")]
    [SwaggerOperation("Get merchant billing purchase transactions")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing purchase transactions returned", typeof(GetPurchaseTransactionsVm))]
    public async Task<ActionResult> GetPurchaseTransactions([FromRoute] int tenantId, [FromRoute] long billingId)
    {
        var transactions = await mediator.Send(new GetPurchaseTransactionsQuery(tenantId, billingId));

        return Ok(transactions);
    }


    [HttpGet("{billingId}/tenants/{tenantId}/refunded-transactions")]
    [SwaggerOperation("Get merchant billing refunded transactions")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing refunded transactions returned", typeof(GetRefundedTransactionsVm))]
    public async Task<ActionResult> GetRefundedTransactions([FromRoute] int tenantId, [FromRoute] long billingId)
    {
        var transactions = await mediator.Send(new GetRefundedTransactionsQuery(tenantId, billingId));

        return Ok(transactions);
    }


    [HttpGet("{billingId}/tenants/{tenantId}/purchase-transactions-commission")]
    [SwaggerOperation("Get merchant billing purchase transactions commission")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing purchase transactions commission returned", typeof(GetPurchaseTransactionsCommissionVm))]
    public async Task<ActionResult> GetPurchaseTransactionsCommission([FromRoute] int tenantId, [FromRoute] long billingId)
    {
        var commission = await mediator.Send(new GetPurchaseTransactionsCommissionQuery(tenantId, billingId));

        return Ok(commission);
    }


    [HttpGet("{billingId}/tenants/{tenantId}/refunded-transactions-commission")]
    [SwaggerOperation("Get merchant billing refunded transactions commission")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing refunded transactions commission returned", typeof(GetRefundedTransactionsCommissionVm))]
    public async Task<ActionResult> GetRefundedTransactionsCommission([FromRoute] int tenantId, [FromRoute] long billingId)
    {
        var commission = await mediator.Send(new GetRefundedTransactionsCommissionQuery(tenantId, billingId));

        return Ok(commission);
    }

    [HttpGet("{billingId}/tenants/{tenantId}/pay-amount/{amount}/payable-amount")]
    [SwaggerOperation("Get billing payable amount")]
    [SwaggerResponse((int)HttpStatusCode.NotFound, "Merchant billing not found")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Merchant billing payable amount returned", typeof(decimal))]
    public async Task<ActionResult<decimal>> GetBillingPayableAmount([FromRoute] int id, [FromRoute] long billingId, [FromRoute] decimal amount)
    {
        var merchantBillingPayableAmount = await mediator.Send(new GetMerchantBillingPayableAmountQuery(id, billingId, amount));

        return Ok(merchantBillingPayableAmount);
    }
}