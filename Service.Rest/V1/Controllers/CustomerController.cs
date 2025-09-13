using Application.Command.BillingCommands;
using Application.Command.CustomerCommands;
using Application.Command.TransactionCommands.Dtos;
using Application.Command.WalletCommands;
using Application.Query.Queries;
using Application.Query.QueryModels;
using Application.Query.ViewModels.Billings;
using Application.Query.ViewModels.Customers;
using Application.Query.ViewModels.Customers.RequestModels;
using Application.Query.ViewModels.Customers.Wallets;
using Application.Query.ViewModels.Wallets;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Rest.V1.RequestModels.Billings;
using Service.Rest.V1.RequestModels.Customers;
using Service.Rest.V1.RequestModels.Wallets;
using Shared.IdentityServerProvider.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/tenant-panel/customers")]
    public class CustomerController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;
        public CustomerController(IMediator mediator, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
        }


        [HttpGet("{id}/merchant-templates")]
        [SwaggerOperation("get  active wallet merchant")]
        [SwaggerResponse((int)HttpStatusCode.OK, "wallet merchant returned", typeof(GetWalletMerchantVm))]
        public async Task<ActionResult<GetWalletMerchantVm>> ActiveWalletMerchant(int id)
        {
            var result = await _mediator.Send(new GetWalletMerchantQuery(id));
            return Ok(result);
        }

        [HttpGet("{id}/merchants")]
        [SwaggerOperation("get  active wallet merchant list")]
        [SwaggerResponse((int)HttpStatusCode.OK, "wallet  returned", typeof(List<GetActiveWalletCategoryListViewModel>))]
        public async Task<ActionResult<List<GetActiveWalletCategoryListViewModel>>> GetActiveWalletMerchantList(int id, [FromQuery] GetActiveWalletMerchantByFilter query)
        {
            var result = await _mediator.Send(new GetActiveWalletMerchantListQuery(id, query.Categories, query.Wallets, query.SaleType, query.SearchValue, _currentUserService.TenantId));
            return Ok(result);
        }

        [HttpGet("{id}/merchant-categories")]
        [SwaggerOperation("get  active wallet category list")]
        [SwaggerResponse((int)HttpStatusCode.OK, "wallet  returned", typeof(List<GetActiveWalletCategoryListViewModel>))]
        public async Task<ActionResult<List<GetActiveWalletCategoryListViewModel>>> GetActiveWalletCategoryList([FromRoute] int id, [FromQuery] int? walletId)
        {
            var result = await _mediator.Send(new GetActiveWalletCategoryListQuery(id, walletId));
            return Ok(result);
        }


        [HttpGet("{id}/merchants/{merchantId}/wallets")]
        [SwaggerOperation("Get wallet by customer id and merchant id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Wallet by customer id and by terminal id  returned", typeof(GetWalletByMerchantIdViewModel))]
        public async Task<ActionResult<GetWalletByMerchantIdViewModel>> GetWalletsByMerchantId(int id, int merchantId)
        {
            var result = await _mediator.Send(new GetWalletByMerchantIdQuery(id, merchantId));
            return Ok(result);
        }


        [HttpGet]
        [ActionName(nameof(GetListAsync))]
        [SwaggerOperation("get List  customer ")]
        [SwaggerResponse((int)HttpStatusCode.OK, "customer returned", typeof(GetCustomersVm))]
        public async Task<ActionResult<GetCustomersVm>> GetListAsync([FromQuery] GetCustomerListModel request)
        {
            var result = await _mediator.Send(new GetCustomerListQuery(request, _currentUserService.TenantId));
            return Ok(result);
        }


        [HttpGet("{id}")]
        [SwaggerOperation("Gets a Customer")]
        [SwaggerResponse((int)HttpStatusCode.OK, "customer returned", typeof(GetCustomerVm))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "customer not found")]
        public async Task<ActionResult<GetCustomerVm>> GetAsync([FromRoute] int id)
        {
            var result = await _mediator.Send(new GetCustomerByIdQuery(id, _currentUserService.TenantId));
            return Ok(result);
        }


        [HttpGet("{id}/wallets/active")]
        [SwaggerOperation("Gets customer Active Wallets")]
        [SwaggerResponse((int)HttpStatusCode.OK, "wallet  returned", typeof(List<GetActiveWalletViewModel>))]
        public async Task<ActionResult<List<GetActiveWalletViewModel>>> GetActiveWalletsList(int id)
        {
            var result = await _mediator.Send(new GetActiveWalletQuery(id));
            return Ok(result);
        }

        [HttpGet("{id}/wallets")]
        [SwaggerOperation("Gets wallets")]
        [SwaggerResponse((int)HttpStatusCode.OK, "wallet  returned", typeof(List<GetWalletsListViewModel>))]
        public async Task<ActionResult<List<GetWalletsListViewModel>>> GetWalletsList(int id, [FromQuery] GetCustomerWalletsRequest request)
        {
            var result = await _mediator.Send(new GetWalletsPaginatedListQuery(id, _currentUserService.TenantId, request));
            return Ok(result);
        }


        [HttpGet("/api/tenant-panel/superapp/customers/{id}/merchants/{merchantId}/wallets")]
        [SwaggerOperation("Get wallets by merchant id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "merchant wallets returned", typeof(List<GetSuperAppWalletByMerchantIdViewModel>))]
        public async Task<ActionResult<List<GetSuperAppWalletByMerchantIdViewModel>>> GetSuperAppWalletsByMerchantId([FromRoute] int id, [FromRoute] int merchantId)
        {
            var result = await _mediator.Send(new GetSuperAppWalletByMerchantIdQuery(id, merchantId, _currentUserService.TenantId));
            return Ok(result);
        }


        [HttpGet("{id}/wallets/{walletId}")]
        [SwaggerOperation("Gets a Wallet")]
        [SwaggerResponse((int)HttpStatusCode.OK, "wallet current state returned", typeof(GetCustomerWalletViewModel))]
        public async Task<ActionResult<GetCustomerWalletViewModel>> GetCustomerWalletAsync(int id, int walletId)
        {
            var result = await _mediator.Send(new GetCustomerWalletQuery(id, walletId));
            return Ok(result);
        }


        [HttpGet]
        [Route("{id}/wallets/{walletId}/transactions")]
        [SwaggerOperation("Gets wallet transactions by id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "customer wallet transactions returned", typeof(GetCustomerWalletTransactionVm))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "customer wallet transactions not found")]
        public async Task<ActionResult<GetCustomerWalletTransactionsVm>> GetWalletTransactionsAsync([FromRoute] int id, [FromRoute] int walletId, [FromQuery] GetWalletTransactionsRequest request)
        {
            var result = await _mediator.Send(new GetCustomerWalletTransactionQuery(id, walletId, request.Types, request.PageSize, request.PageIndex, _currentUserService.TenantId));
            return Ok(result);
        }


        [HttpGet]
        [Route("{id}/wallets/{walletId}/transactions/{transactionId}")]
        [SwaggerOperation("Gets wallet transactions by id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "customer wallet transactions returned", typeof(GetCustomerWalletTransactionDetailVm))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "customer wallet transactions not found")]
        public async Task<ActionResult<GetCustomerWalletTransactionDetailVm>> GetWalletTransactionDetailAsync([FromRoute] int id, [FromRoute] int walletId, [FromRoute] long transactionId)
        {
            var result = await _mediator.Send(new GetCustomerWalletTransactionDetailQuery(id, walletId, transactionId, tenantId: _currentUserService.TenantId));
            return Ok(result);
        }


        [HttpGet]
        [Route("{id}/wallets/{walletId}/installments")]
        [SwaggerOperation("Gets wallet installments")]
        [SwaggerResponse((int)HttpStatusCode.OK, "wallet installments returned", typeof(GetCustomerWalletInstallmentsInfoVm))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "wallet installments not found")]
        public async Task<ActionResult<GetCustomerWalletInstallmentsInfoVm>> GetWalletInstallmentsAsync([FromRoute] int id, [FromRoute] int walletId)
        {
            var result = await _mediator.Send(new GetCustomerWalletInstalllmentsInfoQuery(id, walletId, tenantId: _currentUserService.TenantId));
            return Ok(result);
        }


        [HttpGet]
        [Route("{id}/financial-documents")]
        [SwaggerOperation("Gets customer financial documents")]
        [SwaggerResponse((int)HttpStatusCode.OK, "get customer financial documents", typeof(GetCustomerFinancialDocumentsVm))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "customer financial documents not found")]
        public async Task<ActionResult<GetCustomerFinancialDocumentsVm>> GetFinancialDocumentsAsync([FromRoute] int id, [FromQuery] GetCustomerFinancialDocumentListModel request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetCustomerFinancialDocumentsQuery(id, request.FromDate, request.ToDate, request.Types, request.PageIndex, request.PageSize, request.SearchValue, request.SortColumn, request.SortDirection, _currentUserService.TenantId), cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        [Route("{id}/financial-documents/{financialDocumentId}")]
        [SwaggerOperation("Gets customer financial document")]
        [SwaggerResponse((int)HttpStatusCode.OK, "wallet customer financial document", typeof(CustomerFinancialDocumentDetailQueryModel))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "customer financial document not found")]
        public async Task<ActionResult<CustomerFinancialDocumentDetailQueryModel>> GetFinancialDocumentAsync([FromRoute] int id, [FromRoute] long financialDocumentId)
        {
            var result = await _mediator.Send(new GetCustomerFinancialDocumentQuery(id, financialDocumentId, _currentUserService.TenantId));
            return Ok(result);
        }

        [HttpPut("{id}/wallets/{walletId}/set-default")]
        [SwaggerOperation("Set default wallet")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Updated")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "wallet not found")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
        public async Task<ActionResult> UpdateDefaultWallet(int id, int walletId)
        {
            await _mediator.Send(new UpdateDefaultWalletCommand(walletId, id));
            return Ok();
        }

        [HttpGet("{id}/wallets/{walletId}/details")]
        [SwaggerOperation("gets a customer wallet details")]
        [SwaggerResponse((int)HttpStatusCode.OK, "customer wallet details returned", typeof(CutomerWalletDetailsViewModel))]
        public async Task<ActionResult<CutomerWalletDetailsViewModel>> GetCustomerWalletDetails(int id, [FromRoute] int walletId)
        {
            var result = await _mediator.Send(new GetCustomerWalletDetailsQuery(id, walletId, _currentUserService.TenantId));
            return Ok(result);
        }

        #region CashWallet

        [HttpGet("{id}/cash-wallet")]
        [SwaggerOperation("gets a customer cash wallet")]
        [SwaggerResponse((int)HttpStatusCode.OK, "customer cash wallet returned", typeof(CashWalletViewModel))]
        public async Task<ActionResult<CashWalletViewModel>> GetCustomerCashWallet([FromRoute] int id)
        {
            var result = await _mediator.Send(new GetCustomerCashWalletQuery(id, _currentUserService.TenantId));
            return Ok(result);
        }

        #endregion

        #region BankAccount

        [HttpGet("{id}/bank-accounts")]
        [SwaggerOperation("Get customer bank account")]
        [SwaggerResponse((int)HttpStatusCode.OK, "bank account returned", typeof(List<GetCustomerBankAccountViewModel>))]
        public async Task<ActionResult<List<GetCustomerBankAccountViewModel>>> GetBankAccountsAsync([FromRoute] int id)
        {
            var result = await _mediator.Send(new GetCustomerBankAccountsQuery(id, _currentUserService.TenantId));
            return Ok(result);
        }

        [HttpPost("{id}/bank-accounts")]
        [SwaggerOperation("Create customer bank account")]
        [SwaggerResponse((int)HttpStatusCode.OK, "created", typeof(int))]
        public async Task<ActionResult<int>> CreateBankAccountsAsync([FromRoute] int id, [FromBody] CreateCustomerBankAccountModel request)
        {
            var ibanInquiryResult = await _mediator.Send(new CreateCustomerBankAccountCommand(id, request.Iban, request.ShamsiBirthDate, _currentUserService.TenantId));

            return Ok(ibanInquiryResult);
        }

        [HttpDelete("{id}/bank-accounts/{bankAccountId}")]
        [SwaggerOperation("delete customer bank account")]
        [SwaggerResponse((int)HttpStatusCode.OK, "deleted")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "bank account not found")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
        public async Task<ActionResult<int>> DeleteBankAccountsAsync([FromRoute] int id, [FromRoute] int bankAccountId)
        {
            await _mediator.Send(new DeleteCustomerBankAccountCommand(bankAccountId, id, _currentUserService.TenantId));

            return Ok();
        }

        #endregion

        #region CashOutRequest

        [HttpPost("{id}/cash-out-requests")]
        [SwaggerOperation("Create cash out request")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Created", typeof(long))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
        public async Task<ActionResult<long>> CreateCashOutRequest([FromRoute] int id, [FromBody] CreateCashOutRequestModel request)
        {
            var cashOutId = await _mediator.Send(new CreateCustomerCashOutRequestCommand(request.BankAccountId, request.Amount, id,
                _currentUserService.TenantId));

            return Ok(cashOutId);
        }

        [HttpGet("cash-out-requests")]
        [SwaggerOperation("Get customer cash out request")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Get cash out request", typeof(GetCustomerCashOutRequestsQueryVm))]
        public async Task<ActionResult<GetCustomerCashOutRequestsQueryVm>> GetCashOutRequests([FromQuery] GetCustomersCashOutModel request)
        {
            var result = await _mediator.Send(new GetCustomersCashOutRequestsQuery(request,
                request.Status, request.FromCreateDateTime, request.ToCreateDateTime, _currentUserService.TenantId));

            return Ok(result);
        }

        [HttpPost("{id}/cash-out-requests/{cashOutRequestId}/Approve")]
        [SwaggerOperation("Approve Cash Out Request")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Approve customer returned")]
        public async Task<ActionResult> ApproveCashOutRequestAsync([FromRoute] int id, [FromRoute] int cashOutRequestId, [FromBody] ApproveCustomerCashOutRequestModel request)
        {
            await _mediator.Send(new ApproveCustomerCashOutRequestCommand(id, cashOutRequestId,
                request.BankTransactionCode,
                request.Description,
             _currentUserService.TenantId));

            return Ok();
        }

        [HttpPost("{id}/cash-out-requests/{cashOutRequestId}/Reject")]
        [SwaggerOperation("Reject Cash Out Request")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Reject customer returned")]
        public async Task<ActionResult> RejectCashOutRequestAsync([FromRoute] int id, [FromRoute] int cashOutRequestId, [FromBody] RejectCustomerCashOutRequestModel request)
        {
            await _mediator.Send(new RejectCustomerCashOutRequestCommand(id, cashOutRequestId, request.Description,
                request.RejectReason, _currentUserService.TenantId));

            return Ok();
        }


        #endregion

        [HttpGet]
        [Route("{id}/wallets/{walletId}/balance")]
        [SwaggerOperation("Get customer wallet balance")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Customer wallet balance returned", typeof(decimal))]
        public async Task<ActionResult<decimal>> GetCustomerWalletBalance([FromRoute] int id, [FromRoute] int walletId)
        {
            var result = await _mediator.Send(new GetCustomerWalletBalanceQuery(id, walletId, _currentUserService.TenantId));

            return Ok(result);
        }


        #region Billing

        [HttpGet]
        [Route("{id}/bills")]
        [SwaggerOperation("Gets customer's bills")] // check wallet id and cusstomer
        [SwaggerResponse((int)HttpStatusCode.OK, "customer's bills returned", typeof(GetCustomerBillsVm))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "customer's bills not found", typeof(GetCustomerBillsVm))]
        public async Task<ActionResult<GetCustomerBillsVm>> GetCustomerBillsListAsync([FromRoute] int id, [FromQuery] GetCustomerBillsListModel request)
        {
            var result = await _mediator.Send(new GetCustomerBillsQuery(id, dateType: request.DateType, request, tenantId: _currentUserService.TenantId, request.WalletId));
            return Ok(result);
        }

        [HttpGet("{id}/bills/{billId}")]
        [SwaggerOperation("Gets customer bill")]
        [SwaggerResponse((int)HttpStatusCode.OK, "customer bill returned", typeof(GetCustomerBillDetailsVm))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "customer bill details not found}")]
        public async Task<ActionResult<GetCustomerBillDetailsVm>> GetCustomerBillAsync([FromRoute(Name = "id")] int customerId, [FromRoute] long billId)
        {
            GetCustomerBillDetailsVm result = await _mediator.Send(new GetCustomerBillDetailsQuery(customerId, billId, _currentUserService.TenantId));
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpPost("{customerId}/billings/{billingId}/payment")]
        [SwaggerOperation("Billing payment")]
        [SwaggerResponse((int)HttpStatusCode.Created, "Created", typeof(List<SetTransactionDto>))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "Some validation or business error")]
        public async Task<ActionResult<List<SetTransactionDto>>> BillingPayment(
            [FromRoute] int customerId, [FromRoute] long billingId, CreateBillingPaymentRequest request)
        {
            var setTransactionDtos = await _mediator.Send(new CreateBillingPaymentCommand(
                    request.TenantId,
                    customerId,
                    request.Amount,
                    billingId,
                    request.PaymentId,
                    request.PaymentDetails
                    ));

            return Ok(setTransactionDtos);
        }

        [HttpGet("{id}/validate-billing/{billingId}")]
        [SwaggerOperation("Validate and get billing information")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Billing Information returned", typeof(GetBillingRemainAmount))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Billing not found")]
        public async Task<ActionResult<GetBillingRemainAmount>> GetBilling([FromRoute] int id, [FromRoute] long billingId)
        {
            var result = await _mediator.Send(new GetCustomerBillInfoQuery(billingId, _currentUserService.TenantId, id));
            return Ok(result);
        }
        #endregion
    }
}
