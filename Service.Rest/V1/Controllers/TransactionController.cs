using Application.Command.TransactionCommands;
using Application.Command.TransactionCommands.Dtos;
using Application.Service.Contracts;
using Application.Service.Dtos.FinancialDocuments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Rest.V1.RequestModels.Transactions;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers;

[ApiVersion("1.0")]
[Route("api/transactions")]
[ApiController]
[Authorize]
public class TransactionController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILoanWalletService _walletService;

    public TransactionController(IMediator mediator, ILoanWalletService walletService)
    {
        _mediator = mediator;
        _walletService = walletService;
    }

    [HttpPost("set-transaction")]
    [SwaggerOperation("Validate and set transaction")]
    [SwaggerResponse((int)HttpStatusCode.Created, "Created", typeof(List<SetTransactionDto>))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
    public async Task<ActionResult<List<SetTransactionDto>>> Post(ValidateAndSetTransactionModel request)
    {
        var setTransactionDtos = await _mediator.Send(new ValidateAndSetTransactionCommand(request.TenantId, request.CustomerId,
            request.MerchantId, request.MerchantBranchId, request.Amount, request.PaymentId, request.PurchaseGatewayType,
            request.Payments.Select(x => new PaymentDetailDto
            {
                Amount = x.Amount,
                PaymentDetailId = x.PaymentDetailId,
                Type = x.Type,
                WalletId = x.WalletId
            }).ToList()));

        return Ok(setTransactionDtos);
    }

    [HttpPost("set-payment-transaction")]
    [ActionName(nameof(SetPaymentTransaction))]
    [SwaggerOperation("Set payment transaction")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Ok", typeof(List<SetTransactionDto>))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
    public async Task<ActionResult<List<SetTransactionDto>>> SetPaymentTransaction(SetPaymentTransactionModel request)
    {
        var setTransactionDtos = await _mediator.Send(new SetPaymentTransactionCommand(request.TenantId, request.CustomerId,
            request.Amount, request.PaymentId, request.PaymentServiceType,
            request.PaymentDetails.Select(x => new PaymentDetailDto
            {
                Amount = x.Amount,
                PaymentDetailId = x.PaymentDetailId,
                Type = x.Type,
                WalletId = x.WalletId
            }).ToList()));

        return Ok(setTransactionDtos);
    }

    [HttpPut("reverse")]
    [ActionName(nameof(Reverse))]
    [SwaggerOperation("Reverse transactions")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Updated", typeof(List<ReverseTransactionDto>))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
    public async Task<ActionResult> Reverse(ReverseTransactionModel request)
    {
        var reverseTransactionDtos = await _mediator.Send(new ReverseTransactionCommand(request.PaymentIds));
        return Ok(reverseTransactionDtos);
    }

    [HttpPost("TestGranting")]
    [ActionName(nameof(TestGranting))]
    [SwaggerOperation("Test Granting")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Updated", typeof(List<ReverseTransactionDto>))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
    public async Task<ActionResult> TestGranting(TestGrantingModel request)
    {
        try
        {
            await _walletService.CreateCustomerWalletByGranting(new Shared.EventBus.Events.CgmProcessInstanceAddedEvent
            {
                CreditGrantingProcessId = request.CreditGrantingProcessId,
                CustomerId = request.CustomerId,
                InitialCreditAmount = request.InitialCreditAmount,
                NumberOfInstallment = request.NumberOfInstallment,
                OperationalFee = request.OperationalFee,
                OperationalFeeType = request.OperationalFeeType,
                PlanId = request.PlanId,
                TenantId = request.TenantId,
                UserCreditGrantingProcessId = request.UserCreditGrantingProcessId,
                OperationalFeeAmount = request.OperationalFeeAmount,
                VerificationFeeAmount = request.VerificationFeeAmount
            });
        }
        catch (Exception ex)
        {

            throw;
        }

        return Ok();
    }


    [HttpPost("TestSettlementChequeGranting")]
    [ActionName(nameof(TestSettlementChequeGranting))]
    [SwaggerOperation("Test settlement cheque granting")]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validatio                         vn or business error")]
    public async Task<ActionResult> TestSettlementChequeGranting(TestSettlementChequeGrantingModel request)
    {
        try
        {
            await _walletService.CreateCustomerChequeWalletByGranting(new Shared.EventBus.Events.CgmSettlementChequesProcessInstanceAddedEvent
            {
                CreditGrantingProcessId = request.CreditGrantingProcessId,
                CustomerId = request.CustomerId,
                InitialCreditAmount = request.InitialCreditAmount,
                NumberOfInstallment = request.NumberOfInstallment,
                OperationalFee = request.OperationalFee,
                OperationalFeeType = request.OperationalFeeType,
                PlanId = request.PlanId,
                TenantId = request.TenantId,
                UserCreditGrantingProcessId = request.UserCreditGrantingProcessId,
                OperationalFeeAmount = request.OperationalFeeAmount,
                VerificationFeeAmount = request.VerificationFeeAmount,
                RegistrationDate = request.RegistrationDate,
                ChequeDetails = request.ChequeDetails,
            });
        }
        catch (Exception ex)
        {

            throw;
        }

        return Ok();
    }
}
