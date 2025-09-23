using MediatR;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using Shared.IdentityServerProvider.Contracts;
using Application.Query.Queries.MerchantBilling;
using Application.Query.ViewModels.MerchantBillings;

namespace Service.Rest.V1.Controllers.AdminPanel;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/admin-panel/tenants")]
public sealed class AdminMerchantBillingController(IMediator mediator, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet("{id}/merchants/billings/{billingId}")]
    [SwaggerOperation("Get merchant billing list")]
    [SwaggerResponse((int)HttpStatusCode.NotFound, "merchant billing not found")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing returned", typeof(GetMerchantBillingViewModel))]
    public async Task<ActionResult> GetAsync([FromRoute] int id, [FromRoute] long billingId)
    {
        var billing = await mediator.Send(new GetMerchantBillingQuery(id, billingId));

        return Ok(billing);
    }

    [HttpGet("{id}/merchants/billings/{billingId}/previous-debit")]
    [SwaggerOperation("Get merchant billing previous debit")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing previous debit returned", typeof(GetPreviousDebitViewModel))]
    public async Task<ActionResult> GetPreviousDebit([FromRoute] int id, [FromRoute] long billingId)
    {
        var previousDebit = await mediator.Send(new GetPreviousDebitQuery(id, billingId));

        return Ok(previousDebit);
    }

    [HttpGet("{id}/merchants/billings/{billingId}/additions")]
    [SwaggerOperation("Get merchant billing additions")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing additions returned", typeof(GetAdditionsViewModel))]
    public async Task<ActionResult> GetAdditions([FromRoute] int id, [FromRoute] long billingId)
    {
        var additions = await mediator.Send(new GetAdditionsQuery(id, billingId));

        return Ok(additions);
    }

    [HttpGet("{id}/merchants/billings/{billingId}/deductions")]
    [SwaggerOperation("Get merchant billing deductions")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing deductions returned", typeof(GetDeductionsViewModel))]
    public async Task<ActionResult> GetDeductions([FromRoute] int id, [FromRoute] long billingId)
    {
        var additions = await mediator.Send(new GetDeductionsQuery(id, billingId));

        return Ok(additions);
    }

    [HttpGet("{id}/merchants/billings/{billingId}/refunded-transactions-commission")]
    [SwaggerOperation("Get merchant billing refunded transactions commission")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing refunded transactions commission returned", typeof(GetRefundedTransactionsCommissionViewModel))]
    public async Task<ActionResult> GetRefundedTransactionsCommission([FromRoute] int id, [FromRoute] long billingId)
    {
        var commission = await mediator.Send(new GetRefundedPurchasesCommissionQuery(id, billingId));

        return Ok(commission);
    }

    [HttpGet("{id}/merchants/billings/{billingId}/previous-period-refunded-transactions")]
    [SwaggerOperation("Get merchant billing previous period refunded transactions")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing previous period refunded transactions returned", typeof(GetPreviousPeriodRefundedTransactionsViewModel))]
    public async Task<ActionResult> GetPreviousPeriodRefundedTransactions([FromRoute] int id, [FromRoute] long billingId)
    {
        var refundedPurchases = await mediator.Send(new GetPreviousPeriodRefundedTransactionsQuery(id, billingId));

        return Ok(refundedPurchases);
    }

    [HttpGet("{id}/merchants/billings/{billingId}/current-period-final-commission")]
    [SwaggerOperation("Get merchant billing current period final commission")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing current period final commission returned", typeof(GetCurrentPeriodFinalCommissionViewModel))]
    public async Task<ActionResult> GetCurrentPeriodFinalCommission([FromRoute] int id, [FromRoute] long billingId)
    {
        var commission = await mediator.Send(new GetCurrentPeriodFinalCommissionQuery(id, billingId));

        return Ok(commission);
    }

    [HttpGet("{id}/merchants/billings/{billingId}/current-period-purchase-transactions")]
    [SwaggerOperation("Get merchant billing current period purchase transactions")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing current period purchase transactions returned", typeof(GetCurrentPeriodPurchaseTransactionsViewModel))]
    public async Task<ActionResult> GetCurrentPeriodPurchaseTransactions([FromRoute] int id, [FromRoute] long billingId)
    {
        var currentPeriodTransactions = await mediator.Send(new GetCurrentPeriodFinalCommissionQuery(id, billingId));

        return Ok(currentPeriodTransactions);
    }
}