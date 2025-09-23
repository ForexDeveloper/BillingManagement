using Application.Query.Queries.MerchantBilling;
using Application.Query.ViewModels.Billings;
using Application.Query.ViewModels.MerchantBillings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Rest.V1.RequestModels.Billings;
using Shared.IdentityServerProvider.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/tenant-panel/merchants/billings")]
public sealed class MerchantBillingController(IMediator mediator, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation("Get merchant billing list")]
    [Route("/api/tenant-panel/merchants/{id}/billings")]
    [SwaggerResponse((int)HttpStatusCode.NotFound, "merchant billing list not found")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing list returned", typeof(GetBillingsViewModel))]
    public async Task<ActionResult> GetListAsync([FromRoute] int id, [FromQuery] GetBillingsRequest request)
    {
        var billings = await mediator.Send(new GetMerchantBillingsQuery(currentUserService.TenantId, id, request.Code,
            request.Status, request.PageSize, request.PageIndex));

        return Ok(billings);
    }

    [HttpGet("{id}")]
    [SwaggerOperation("Get merchant billing")]
    [SwaggerResponse((int)HttpStatusCode.NotFound, "merchant billing not found")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing returned", typeof(GetMerchantBillingViewModel))]
    public async Task<ActionResult> GetAsync(long id)
    {
        var billing = await mediator.Send(new GetMerchantBillingQuery(id, currentUserService.TenantId));

        return Ok(billing);
    }

    [HttpGet("{id}/previous-debit")]
    [SwaggerOperation("Get merchant billing previous debit")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing previous debit returned", typeof(GetPreviousDebitViewModel))]
    public async Task<ActionResult> GetPreviousDebit(long id)
    {
        var previousDebit = await mediator.Send(new GetPreviousDebitQuery(id, currentUserService.TenantId));

        return Ok(previousDebit);
    }

    [HttpGet("{id}/additions")]
    [SwaggerOperation("Get merchant billing additions")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing additions returned", typeof(GetAdditionsViewModel))]
    public async Task<ActionResult> GetAdditions(long id)
    {
        var additions = await mediator.Send(new GetAdditionsQuery(id, currentUserService.TenantId));

        return Ok(additions);
    }

    [HttpGet("{id}/deductions")]
    [SwaggerOperation("Get merchant billing deductions")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing deductions returned", typeof(GetDeductionsViewModel))]
    public async Task<ActionResult> GetDeductions(long id)
    {
        var additions = await mediator.Send(new GetDeductionsQuery(id, currentUserService.TenantId));

        return Ok(additions);
    }

    [HttpGet("{id}/refunded-transactions-commission")]
    [SwaggerOperation("Get merchant billing refunded transactions commission")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing refunded transactions commission returned", typeof(GetRefundedTransactionsCommissionViewModel))]
    public async Task<ActionResult> GetRefundedTransactionsCommission(long id)
    {
        var commission = await mediator.Send(new GetRefundedPurchasesCommissionQuery(id, currentUserService.TenantId));

        return Ok(commission);
    }

    [HttpGet("{id}/previous-period-refunded-transactions")]
    [SwaggerOperation("Get merchant billing previous period refunded transactions")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing previous period refunded transactions returned", typeof(GetPreviousPeriodRefundedTransactionsViewModel))]
    public async Task<ActionResult> GetPreviousPeriodRefundedTransactions(long id)
    {
        var refundedPurchases = await mediator.Send(new GetPreviousPeriodRefundedTransactionsQuery(id, currentUserService.TenantId));

        return Ok(refundedPurchases);
    }

    [HttpGet("{id}/current-period-final-commission")]
    [SwaggerOperation("Get merchant billing current period final commission")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing current period final commission returned", typeof(GetCurrentPeriodFinalCommissionViewModel))]
    public async Task<ActionResult> GetCurrentPeriodFinalCommission(long id)
    {
        var commission = await mediator.Send(new GetCurrentPeriodFinalCommissionQuery(id, currentUserService.TenantId));

        return Ok(commission);
    }

    [HttpGet("{id}/current-period-purchase-transactions")]
    [SwaggerOperation("Get merchant billing current period purchase transactions")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing current period purchase transactions returned", typeof(GetCurrentPeriodPurchaseTransactionsViewModel))]
    public async Task<ActionResult> GetCurrentPeriodPurchaseTransactions([FromRoute] long id)
    {
        var currentPeriodTransactions = await mediator.Send(new GetCurrentPeriodFinalCommissionQuery(id, currentUserService.TenantId));

        return Ok(currentPeriodTransactions);
    }
}