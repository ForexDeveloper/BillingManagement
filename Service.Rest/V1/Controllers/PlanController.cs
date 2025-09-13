using Application.Command.PlanCommands;
using Application.Query.Queries;
using Application.Query.ViewModels.Plans;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Service.Rest.V1.RequestModels.Plans;
using Service.Rest.V1.RequestModels.Transactions;
using Shared.IdentityServerProvider.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/tenant-panel/plans")]
    [ApiController]
    //[Authorize]
    public class PlanController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;

        public PlanController(IMediator mediator, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
        }

        [HttpPost]
        [SwaggerOperation("Create a new plan")]
        [SwaggerResponse((int)HttpStatusCode.Created, "Created", typeof(int))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
        public async Task<ActionResult<int>> Post(CreatePlanModel request)
        {
            var planId = await _mediator.Send(new CreatePlanCommand(request.WalletConfigurationId, request.WalletLogo,
             request.Title, request.MaxDailyWithdrawal, request.MaxWallet, request.MaxTotalCredit,
             request.BillingPeriodType, request.BillingPeriod, request.BillingPeriodStartDate,
             request.GracePeriod, request.PaymentType, request.InstallmentBreakType, request.InstallmentBreak,
             request.InstallmentPaymentMethod, request.MaxDailyDeposit,
             request.MaxDailyTransactionCount, request.BackgroundColor1, request.BackgroundColor2,
             request.TextColor, request.Description, request.Link, request.PlanClosedloops,
             request.PlanDetails, request.TermsAndConditions, _currentUserService.TenantId));
            return CreatedAtAction(nameof(GetAsync), new { id = planId }, planId);
        }


        [HttpPut]
        [Route("{id}")]
        [SwaggerOperation("Update a plan  by id")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "plan info not found")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
        public async Task<ActionResult> Put(UpdatePlanModel request, [FromRoute] int id)
        {
            var planId = await _mediator.Send(new UpdatePlanCommand(id, request.WalletConfigurationId, request.WalletLogo,
            request.Title, request.MaxDailyWithdrawal, request.MaxWallet, request.MaxTotalCredit,
            request.BillingPeriodType, request.BillingPeriod, request.BillingPeriodStartDate,
            request.GracePeriod, request.PaymentType, request.InstallmentBreakType, request.InstallmentBreak,
            request.InstallmentPaymentMethod, request.MaxDailyDeposit,
            request.MaxDailyTransactionCount, request.BackgroundColor1, request.BackgroundColor2,
            request.TextColor, request.Description, request.Link, request.PlanClosedloops,
            request.PlanDetails, request.TermsAndConditions, _currentUserService.TenantId));
            return Ok();
        }

        [HttpPut("cash-wallet/{id}")]
        [SwaggerOperation("Update a plan cash wallet by id")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "plan info not found")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
        public async Task<ActionResult> UpdateCashWallet(UpdateCashWalletConfigurationPlanModel request, [FromRoute] int id)
        {
            var planId = await _mediator.Send(new UpdateCashWalletConfigurationPlanCommand(id, request.MaxWallet,
                request.MaxDailyWithdrawal, request.MaxDailyDeposit, request.MaxDailyTransactionCount,
                request.TermsAndConditions, _currentUserService.TenantId));
            return Ok();
        }

        [HttpGet]
        [ActionName(nameof(GetAsync))]
        [Route("{id}")]
        [SwaggerOperation("get a plan by id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "plan returned", typeof(PlanViewModel))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "plan not found")]
        public async Task<ActionResult<PlanViewModel>> GetAsync([FromRoute] int id)
        {
            var result = await _mediator.Send(new GetPlanByIdQuery(id, _currentUserService.TenantId));
            return Ok(result);
        }

        [HttpGet()]
        [SwaggerOperation("get plan list By tenant id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "plan list returned", typeof(GetPlanForGridViewModel))]
        public async Task<ActionResult<GetPlanForGridViewModel>> GetListAsync([FromQuery] GetPlanBaseModel request)
        {
            var result = await _mediator.Send(new GetAllPlanQuery(request, _currentUserService.TenantId, request.WalletConfigurationId));
            return Ok(result);
        }


        [HttpGet]
        [Route("simple-list")]
        [SwaggerOperation("get plans By tenant id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "plan list returned", typeof(PlanSimpleListViewModel))]
        public async Task<ActionResult<List<PlanSimpleListViewModel>>> GetPlansSimpleListAsync()
        {
            var result = await _mediator.Send(new GetPlanSimpleListQuery(_currentUserService.TenantId));
            return Ok(result);
        }

        [HttpPost("plan-financials")]
        [SwaggerOperation("Plan financial details")]
        [SwaggerResponse((int)HttpStatusCode.OK, "plan financial details returned", typeof(List<PlanFinancialDetailsVm>))]
        public async Task<ActionResult<List<PlanFinancialDetailsVm>>> GetPlansFinancialetails(List<PlanFinancialDetailsModel> request)
        {
            var planFinancialDetailsDtos = request.Select(x => new PlanFinancialDetailsDto
            {
                CreditAmount = x.CreditAmount,
                OperationalFeeType = x.OperationalFeeType,
                PlanDetailInstallmentId = x.PlanDetailInstallmentId
            }).ToList();

            var planFinancialDetails = await _mediator.Send(new GetPlanFinancialDetailsQuery(planFinancialDetailsDtos));

            return Ok(planFinancialDetails);
        }

        [HttpPatch("{id}/reserved-credit/increase")]
        [SwaggerOperation("increase plan reserved credit amount")]
        [SwaggerResponse((int)HttpStatusCode.OK, "increase plan reserved credit amount")]
        public async Task<ActionResult> IncreasePlanReservedCreditAmount([FromRoute] int id, [FromBody] IncreasePlanReservedCreditAmountModel request)
        {
            await _mediator.Send(new IncreasePlanReservedAmountCommand(id, request.RequestedCreditAmount));
            return Ok();
        }

        [HttpPatch("{id}/reserved-credit/decrease")]
        [SwaggerOperation("decrease plan reserved credit amount")]
        [SwaggerResponse((int)HttpStatusCode.OK, "decrease plan reserved credit amount")]
        public async Task<ActionResult> DecreasePlanReservedCreditAmount([FromRoute] int id, [FromBody] DecreasePlanReservedCreditAmountModel request)
        {
            await _mediator.Send(new DecreasePlanReservedAmountCommand(id, request.RequestedCreditAmount));
            return Ok();
        }

        [HttpGet("{id}/plan-installments")]
        [SwaggerOperation("Plan installments")]
        [SwaggerResponse((int)HttpStatusCode.OK, "plan installments returned", typeof(List<PlanInstallmentsVm>))]
        public async Task<ActionResult<List<PlanInstallmentsVm>>> GetPlanInstallments([FromRoute] int id, [FromQuery] GetPlanInstallmentsModel request)
        {
            var planInstallments = await _mediator.Send(new GetPlanInstallmentsQuery(id, request.NumberOfInstallments, request.Amount, request.OperationalFeeType));
            return Ok(planInstallments);
        }
    }
}