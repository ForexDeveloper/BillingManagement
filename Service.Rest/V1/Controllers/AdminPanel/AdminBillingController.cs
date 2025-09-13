//using Application.Command.BillingCommands;
//using MediatR;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Swashbuckle.AspNetCore.Annotations;
//using System.Net;

//namespace Service.Rest.V1.Controllers.AdminPanel;

//[ApiVersion("1.0")]
//[Route("api/admin-panel/billings")]
//[ApiController]
//[Authorize]
//public class AdminBillingController : ControllerBase
//{
//    private readonly IMediator _mediator;

//    public AdminBillingController(IMediator mediator)
//    {
//        _mediator = mediator;
//    }

//    [HttpPut]
//    [SwaggerOperation("Update billing and installment dates")]
//    [SwaggerResponse((int)HttpStatusCode.OK, "Ok", typeof(void))]
//    public async Task<ActionResult> Put(UpdateBillingForTestCommand request)
//    {
//        await _mediator.Send(request);
//        return Ok();
//    }
//}
