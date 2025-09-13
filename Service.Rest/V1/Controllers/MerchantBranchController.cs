using Application.Command.TransactionCommands;
using Application.Query.Queries;
using Application.Query.ViewModels.FinancialDocuments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Rest.V1.RequestModels.Merchants;
using Service.Rest.V1.RequestModels.Transactions;
using Shared.IdentityServerProvider.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers;

[ApiVersion("1.0")]
[Route("api/tenant-panel/merchant-branches")]
[ApiController]
[Authorize]
public class MerchantBranchController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public MerchantBranchController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HttpPost("{id}/refunds")]
    [SwaggerOperation("Refund transactions")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Refunded")]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
    public async Task<ActionResult<bool>> Refund([FromRoute] int id, [FromBody] RefundTransactionModel request)
    {
        await _mediator.Send(new RefundTransactionCommand(request.FinancialDocumentId, null, request.Amount, request.Reason, request.Description, _currentUserService.TenantId, id, false));
        return Ok();
    }

    [HttpGet("{id}/refunds/{financialDocumentId}")]
    [ActionName(nameof(GetRefundDetails))]
    [SwaggerOperation("Get refund details by financial document id")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Refund details returned", typeof(RefundFinancialDocumentViewModel))]
    public async Task<ActionResult> GetRefundDetails([FromRoute] int id, [FromRoute] long financialDocumentId)
    {
        var result = await _mediator.Send(new GetRefundFinancialDocumentByIdQuery(financialDocumentId, null, _currentUserService.TenantId, id, false));
        return Ok(result);
    }

    [HttpGet("{id}/purchases")]
    [SwaggerOperation("Get  purchases list by merchant-branchId")]
    [SwaggerResponse((int)HttpStatusCode.OK, "merchant branch purchases list returned", typeof(GetMerchantPurchasesViewModel))]
    public async Task<ActionResult<GetMerchantPurchasesViewModel>> GetPurchasesList([FromRoute] int id, [FromQuery] GetMerchantBranchPurchasesModel query)
    {
        var result = await _mediator.Send(new GetMerchantPurchasesQuery(query, query.FromDate, query.ToDate, query.WalletIds,
            new List<int> { id }, tenantId: _currentUserService.TenantId));
        return Ok(result);
    }


    [HttpGet("{id:int}/active-contract-exists")]
    [SwaggerOperation("Has tenant merchant contract")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Has tenant merchant contractn returned", typeof(bool))]
    [SwaggerResponse((int)HttpStatusCode.NotFound, "Tenant merchant contractn not found")]
    public async Task<ActionResult<bool>> HasActiveTenantMerchantContract([FromRoute] int id)
    {
        return Ok(await _mediator.Send(new HasActiveTenantMerchantContractByMerchantBranchIdQuery(id, _currentUserService.TenantId)));
    }
}
