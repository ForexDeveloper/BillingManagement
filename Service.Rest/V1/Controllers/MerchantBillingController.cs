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
        var billing = await mediator.Send(new GetMerchantBillingQuery(currentUserService.TenantId, id));

        return Ok(billing);
    }


    [HttpGet("{id}/additions")]
    [SwaggerOperation("Get merchant billing additions")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing additions returned", typeof(GetAdditionsViewModel))]
    public async Task<ActionResult> GetAdditions(long id)
    {
        var additions = await mediator.Send(new GetAdditionsQuery(currentUserService.TenantId, id));

        return Ok(additions);
    }


    [HttpPut("{id}/additions")]
    [SwaggerOperation("Update merchant billing additions")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing additions updated", typeof(long))]
    public async Task<ActionResult> UpdateAdditions([FromRoute] long id, UpdateBillingAdditionsRequest request)
    {
        var additions = await mediator.Send(new UpdateBillingAdditionsCommand(currentUserService.TenantId, id, request.Amount, request.Description));

        return Ok(additions);
    }


    [HttpGet("{id}/deductions")]
    [SwaggerOperation("Get merchant billing deductions")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing deductions returned", typeof(GetDeductionsViewModel))]
    public async Task<ActionResult> GetDeductions(long id)
    {
        var additions = await mediator.Send(new GetDeductionsQuery(currentUserService.TenantId, id));

        return Ok(additions);
    }


    [HttpPut("{id}/deductions")]
    [SwaggerOperation("Update merchant billing deductions")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing deductions updated", typeof(long))]
    public async Task<ActionResult> UpdateDeductions([FromRoute] long id, UpdateBillingDeductionsRequest request)
    {
        var additions = await mediator.Send(new UpdateBillingDeductionsCommand(currentUserService.TenantId, id, request.Amount, request.Description));

        return Ok(additions);
    }


    [HttpGet("{id}/previous-debit")]
    [SwaggerOperation("Get merchant billing previous debit")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing previous debit returned", typeof(GetPreviousDebitViewModel))]
    public async Task<ActionResult> GetPreviousDebit(long id)
    {
        var previousDebit = await mediator.Send(new GetPreviousDebitQuery(currentUserService.TenantId, id));

        return Ok(previousDebit);
    }


    [HttpGet("{id}/previous-credit")]
    [SwaggerOperation("Get merchant billing previous credit")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing previous credit returned", typeof(GetPreviousCreditViewModel))]
    public async Task<ActionResult> GetPreviousCredit(long id)
    {
        var previousCredit = await mediator.Send(new GetPreviousCreditQuery(currentUserService.TenantId, id));

        return Ok(previousCredit);
    }


    [HttpGet("{id}/purchase-transactions")]
    [SwaggerOperation("Get merchant billing purchase transactions")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing purchase transactions returned", typeof(GetPurchaseTransactionsViewModel))]
    public async Task<ActionResult> GetPurchaseTransactions([FromRoute] long id)
    {
        var transactions = await mediator.Send(new GetPurchaseTransactionsQuery(currentUserService.TenantId, id));

        return Ok(transactions);
    }


    [HttpGet("{id}/refunded-transactions")]
    [SwaggerOperation("Get merchant billing refunded transactions")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing refunded transactions returned", typeof(GetRefundedTransactionsViewModel))]
    public async Task<ActionResult> GetRefundedTransactions(long id)
    {
        var transactions = await mediator.Send(new GetRefundedTransactionsQuery(currentUserService.TenantId, id));

        return Ok(transactions);
    }


    [HttpGet("{id}/purchase-transactions-commission")]
    [SwaggerOperation("Get merchant billing purchase transactions commission")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing purchase transactions commission returned", typeof(GetPurchaseTransactionsCommissionViewModel))]
    public async Task<ActionResult> GetPurchaseTransactionsCommission(long id)
    {
        var commission = await mediator.Send(new GetPurchaseTransactionsCommissionQuery(currentUserService.TenantId, id));

        return Ok(commission);
    }


    [HttpGet("{id}/refunded-transactions-commission")]
    [SwaggerOperation("Get merchant billing refunded transactions commission")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant billing refunded transactions commission returned", typeof(GetRefundedTransactionsCommissionViewModel))]
    public async Task<ActionResult> GetRefundedTransactionsCommission(long id)
    {
        var commission = await mediator.Send(new GetRefundedTransactionsCommissionQuery(currentUserService.TenantId, id));

        return Ok(commission);
    }
}