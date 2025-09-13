using Application.Command.TransactionCommands;
using Application.Query.Queries;
using Application.Query.ViewModels.FinancialDocuments;
using Application.Query.ViewModels.Merchants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Rest.V1.RequestModels.Merchants;
using Service.Rest.V1.RequestModels.Transactions;
using Shared.IdentityServerProvider.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/tenant-panel/merchants")]
    [ApiController]
    [Authorize]
    public class MerchantController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;

        public MerchantController(IMediator mediator, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        [SwaggerOperation("get List  merchant ")]
        [SwaggerResponse((int)HttpStatusCode.OK, "merchant returned", typeof(GetMerchantListVm))]
        public async Task<ActionResult<GetMerchantListVm>> GetListAsync()
        => Ok(await _mediator.Send(new GetMerchantListQuery(_currentUserService.TenantId)));


        [HttpGet("{branchTerminalId}/validate")]
        [SwaggerOperation("Validate merchant branch")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Merchant branch validation returned", typeof(bool))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Merchant branch not found")]
        public async Task<ActionResult<bool>> ValidateMerchantBranch([FromRoute] long branchTerminalId)
        {
            return Ok(await _mediator.Send(new ValidateMerchantBranchQuery(branchTerminalId)));
        }

        [HttpPost("{merchantId}/refunds")]
        [SwaggerOperation("Refund transactions")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Refunded")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
        public async Task<ActionResult<bool>> Refund([FromRoute] int merchantId, [FromBody] RefundTransactionModel request)
        {
            await _mediator.Send(new RefundTransactionCommand(request.FinancialDocumentId, merchantId, request.Amount, request.Reason, request.Description, _currentUserService.TenantId, isMerchant: true));
            return Ok();
        }

        [HttpGet("{merchantId}/refunds/{financialDocumentId}")]
        [ActionName(nameof(GetRefundDetails))]
        [SwaggerOperation("Get refund details by financial document id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Refund details returned", typeof(RefundFinancialDocumentViewModel))]
        public async Task<ActionResult> GetRefundDetails([FromRoute] int merchantId, [FromRoute] long financialDocumentId)
        {
            var result = await _mediator.Send(new GetRefundFinancialDocumentByIdQuery(financialDocumentId, merchantId, _currentUserService.TenantId, isMerchant: true));
            return Ok(result);
        }

        [HttpGet("{id}/purchases")]
        [SwaggerOperation("Get  purchases list by id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "merchant purchases list returned", typeof(GetMerchantPurchasesViewModel))]
        public async Task<ActionResult<GetMerchantPurchasesViewModel>> GetPurchasesList([FromRoute] int id, [FromQuery] GetMerchantPurchasesModel query)
        {
            var result = await _mediator.Send(new GetMerchantPurchasesQuery(query, query.FromDate, query.ToDate, query.WalletIds,
                query.MerchantBrancheIds, id, _currentUserService.TenantId));
            return Ok(result);
        }

        [HttpGet("{id}/branches")]
        [ActionName(nameof(GetMerchantBranchesAsync))]
        [SwaggerOperation("Get a merchant branch by id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "merchant branch returned", typeof(GetMerchantBranchViewModel))]
        public async Task<ActionResult<List<GetMerchantBranchViewModel>>> GetMerchantBranchesAsync([FromRoute] int id)
        {
            var result = await _mediator.Send(new GetMerchantBranchesByTenantIdQuery(id, _currentUserService.TenantId));
            return Ok(result);
        }

        [HttpGet("{id:int}/active-contract-exists")]
        [SwaggerOperation("Has tenant merchant contract")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Has tenant merchant contractn returned", typeof(bool))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Tenant merchant contractn not found")]
        public async Task<ActionResult<bool>> HasActiveTenantMerchantContract([FromRoute] int id)
        {
            return Ok(await _mediator.Send(new HasActiveTenantMerchantContractByMerchantIdQuery(id, _currentUserService.TenantId)));
        }
    }
}
