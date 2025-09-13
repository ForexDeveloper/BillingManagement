using Application.Command.PlanCommands;
using Application.Query.Queries;
using Application.Query.ViewModels.Plans;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Rest.V1.RequestModels.Plans;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers.AdminPanel
{
    [ApiVersion("1.0")]
    [Route("api/admin-panel/plans")]
    [ApiController]
    [Authorize]
    public class AdminPlanController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AdminPlanController(IMediator mediator)
        {
            _mediator = mediator;
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
            request.PlanDetails, request.TermsAndConditions));
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
            request.PlanDetails, request.TermsAndConditions));
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
                request.TermsAndConditions));
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
            var result = await _mediator.Send(new GetPlanByIdQuery(id));
            return Ok(result);
        }

        [HttpGet]
        [SwaggerOperation("get plan list By tenant id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "plan list returned", typeof(GetPlanForGridViewModel))]
        public async Task<ActionResult<GetPlanForGridViewModel>> GetListAsync([FromQuery] GetPlanModel request)
        {
            var result = await _mediator.Send(new GetAllPlanQuery(request, request.TenantId, request.WalletConfigurationId));
            return Ok(result);
        }

        [HttpGet]
        [Route("simple-list")]
        [SwaggerOperation("get plans By tenant id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "plan list returned", typeof(PlanSimpleListViewModel))]
        public async Task<ActionResult<List<PlanSimpleListViewModel>>> GetPlansSimpleListAsync([FromQuery] GetPlanSimpleListModel request)
        {
            var result = await _mediator.Send(new GetPlanSimpleListQuery(request.TenantId));
            return Ok(result);
        }
    }

}