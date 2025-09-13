using Application.Query.Queries;
using Application.Query.QueryModels;
using Application.Query.ViewModels.Customers;
using Application.Query.ViewModels.Customers.RequestModels;
using Application.Query.ViewModels.Customers.Wallets;
using Application.Query.ViewModels.Wallets;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Rest.V1.RequestModels.Customers;
using Shared.IdentityServerProvider.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers.AdminPanel
{
    [ApiVersion("1.0")]
    [Route("api/admin-panel/customers")]
    [ApiController]
    [Authorize]
    public class AdminCustomerController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AdminCustomerController(IMediator mediator, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
        }
        private readonly ICurrentUserService _currentUserService;


        [HttpGet]
        [SwaggerOperation("get List customer ")]
        [SwaggerResponse((int)HttpStatusCode.OK, "customer returned", typeof(GetCustomersVm))]
        public async Task<ActionResult<GetCustomersVm>> GetListAsync([FromQuery] GetCustomerListModel request)
        {
            var result = await _mediator.Send(new GetCustomerListQuery(request, request.TenantId));
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetAsync))]
        [SwaggerOperation("Get a customer by id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "customer returned", typeof(GetCustomerVm))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "customer not found")]
        public async Task<ActionResult<GetCustomerVm>> GetAsync([FromRoute] int id)
        {
            var result = await _mediator.Send(new GetCustomerByIdQuery(id));
            return Ok(result);
        }

        [HttpGet("{id}/wallets")]
        [SwaggerOperation("get wallets")]
        [SwaggerResponse((int)HttpStatusCode.OK, "wallet  returned", typeof(GetCustomerWalletsVm))]
        public async Task<ActionResult<GetCustomerWalletsVm>> GetWalletsList([FromRoute] int id, [FromQuery] GetCustomerWalletsRequest request)
        {
            var result = await _mediator.Send(new GetWalletsPaginatedListQuery(id, null, request));
            return Ok(result);
        }

        [HttpGet("{id}/wallets/{walletId}/details")]
        [SwaggerOperation("gets a customer wallet details")]
        [SwaggerResponse((int)HttpStatusCode.OK, "customer wallet details returned", typeof(CutomerWalletDetailsViewModel))]
        public async Task<ActionResult<CutomerWalletDetailsViewModel>> GetCustomerWalletDetails(int id, int walletId)
        {
            var result = await _mediator.Send(new GetCustomerWalletDetailsQuery(id, walletId));
            return Ok(result);
        }

        [HttpGet]
        [ActionName(nameof(GetWalletTransactionsAsync))]
        [Route("{id}/wallets/{walletId}/transactions")]
        [SwaggerOperation("get wallet transactions by id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "customer wallet transactions returned", typeof(GetCustomerWalletTransactionVm))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "customer wallet transactions not found")]
        public async Task<ActionResult<GetCustomerWalletTransactionsVm>> GetWalletTransactionsAsync([FromRoute] int id, [FromRoute] int walletId, [FromQuery] GetWalletTransactionsRequest request)
        {
            var result = await _mediator.Send(new GetCustomerWalletTransactionQuery(id, walletId, request.Types, request.PageSize, pageIndex: request.PageIndex));
            return Ok(result);
        }


        [HttpGet]
        [ActionName(nameof(GetWalletTransactionDetailAsync))]
        [Route("{id}/wallets/{walletId}/transactions/{transactionId}")]
        [SwaggerOperation("get wallet transactions by id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "customer wallet transactions returned", typeof(GetCustomerWalletTransactionDetailVm))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "customer wallet transactions not found")]
        public async Task<ActionResult<GetCustomerWalletTransactionDetailVm>> GetWalletTransactionDetailAsync([FromRoute] int id, [FromRoute] int walletId, [FromRoute] long transactionId)
        {
            var result = await _mediator.Send(new GetCustomerWalletTransactionDetailQuery(id, walletId, transactionId));
            return Ok(result);
        }

        [HttpGet]
        [ActionName(nameof(GetWalletInstallmentsAsync))]
        [Route("{id}/wallets/{walletId}/installments")]
        [SwaggerOperation("get wallet installments")]
        [SwaggerResponse((int)HttpStatusCode.OK, "wallet installments returned", typeof(GetCustomerWalletInstallmentsInfoVm))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "wallet installments not found")]
        public async Task<ActionResult<GetCustomerWalletInstallmentsInfoVm>> GetWalletInstallmentsAsync([FromRoute] int id, [FromRoute] int walletId)
        {

            var result = await _mediator.Send(new GetCustomerWalletInstalllmentsInfoQuery(id, walletId));
            return Ok(result);
        }

        [HttpGet]
        [Route("{id}/financial-documents")]
        [ActionName(nameof(GetAsync))]
        [SwaggerOperation("get customer financial documents")]
        [SwaggerResponse((int)HttpStatusCode.OK, "get customer financial documents", typeof(GetCustomerFinancialDocumentsVm))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "customer financial documents not found")]
        public async Task<ActionResult<GetCustomerFinancialDocumentsVm>> GetFinancialDocumentsAsync([FromRoute] int id, [FromQuery] GetCustomerFinancialDocumentListModel request, CancellationToken cancellationToken)
        {
            var result = await
                _mediator.Send(new GetCustomerFinancialDocumentsQuery(id, request.FromDate,
                request.ToDate, request.Types, request.PageIndex, request.PageSize, request.SearchValue,
                request.SortColumn, request.SortDirection), cancellationToken);
            return Ok(result);
        }


        [HttpGet]
        [Route("{id}/financial-documents/{financialDocumentId}")]
        [ActionName(nameof(GetAsync))]
        [SwaggerOperation("get customer financial document")]
        [SwaggerResponse((int)HttpStatusCode.OK, "wallet customer financial document", typeof(CustomerFinancialDocumentDetailQueryModel))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "customer financial document not found")]
        public async Task<ActionResult<CustomerFinancialDocumentDetailQueryModel>> GetFinancialDocumentAsync([FromRoute] int id, [FromRoute] long financialDocumentId)
        {
            var result = await _mediator.Send(new GetCustomerFinancialDocumentQuery(id, financialDocumentId));
            return Ok(result);
        }

        [HttpGet("cash-out-requests")]
        [SwaggerOperation("get customer cash out request")]
        [SwaggerResponse((int)HttpStatusCode.OK, "get cash out request", typeof(GetCustomerCashOutRequestsQueryVm))]
        public async Task<ActionResult<GetCustomerCashOutRequestsQueryVm>> GetCashOutRequests([FromQuery] AdminGetCustomersCashOutModel request)
        {
            var result = await _mediator.Send(new GetCustomersCashOutRequestsQuery(request,
                request.Status, request.FromCreateDateTime, request.ToCreateDateTime, request.TenantId));

            return Ok(result);
        }



        #region Billing

        [HttpGet]
        [Route("{id}/bills")]
        [SwaggerOperation("Gets customer's bills")]
        [SwaggerResponse((int)HttpStatusCode.OK, "customer's bills returned", typeof(GetCustomerBillsVm))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "customer's bills not found")]

        public async Task<ActionResult<GetCustomerBillsVm>> GetBillsAsync([FromRoute] int id, [FromQuery] GetCustomerBillsListModel request)
        {
            var result = await _mediator.Send(new GetCustomerBillsQuery(customerId: id, request.DateType, request, tenantId: null, request.WalletId));
            return result == null ? (ActionResult<GetCustomerBillsVm>)NotFound() : (ActionResult<GetCustomerBillsVm>)Ok(result);
        }

        [HttpGet("{id}/bills/{billId}")]
        [SwaggerOperation("Gets a customer bill ")]
        [SwaggerResponse((int)HttpStatusCode.OK, "a customer bill returned", typeof(GetCustomerBillDetailsVm))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "a customer bill not found}")]
        public async Task<ActionResult<GetCustomerBillDetailsVm>> GetCustomerBillDetailsAsync([FromRoute(Name = "id")] int customerId, [FromRoute] long billId, [FromQuery] int? tenantId)
        {
            GetCustomerBillDetailsVm result = await _mediator.Send(new GetCustomerBillDetailsQuery(customerId, billId, tenantId));
            return result == null ? (ActionResult<GetCustomerBillDetailsVm>)NotFound() : (ActionResult<GetCustomerBillDetailsVm>)Ok(result);
        }
        [HttpGet("{id}/bills/{billId}/payment-detail/{paymentId}")]
        [SwaggerOperation("Gets a customer bill ")]
        [SwaggerResponse((int)HttpStatusCode.OK, "a customer bill returned", typeof(GetCustomerBillDetailsVm))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "a customer bill not found}")]
        public async Task<ActionResult<GetCustomerBillPaymentDetailVm>> GetCustomerBillPaymentDetailAsync([FromRoute(Name = "id")] int customerId, [FromRoute] long billId, [FromRoute] int paymentId, [FromQuery] int? tenantId)
        {
            var result = await _mediator.Send(new GetCustomerBillPaymentDetailQuery(customerId, billId, tenantId, paymentId));
            return result == null ? (ActionResult<GetCustomerBillPaymentDetailVm>)NotFound() : (ActionResult<GetCustomerBillPaymentDetailVm>)Ok(result);
        }

        #endregion
    }
}
