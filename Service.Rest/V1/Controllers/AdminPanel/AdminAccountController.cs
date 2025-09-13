//using Application.Command.AccountCommands;
//using MediatR;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Swashbuckle.AspNetCore.Annotations;
//using System.Net;

//namespace Service.Rest.V1.Controllers.AdminPanel;

//[ApiVersion("1.0")]
//[Route("api/admin-panel/accounts")]
//[ApiController]
//[Authorize]
//public class AdminAccountController : ControllerBase
//{
//    private readonly IMediator _mediator;

//    public AdminAccountController(IMediator mediator)
//    {
//        _mediator = mediator;
//    }

//    [HttpPut]
//    [SwaggerOperation("Update checksums")]
//    [SwaggerResponse((int)HttpStatusCode.OK, "Ok", typeof(void))]
//    public async Task<ActionResult> Put(UpdateAccountCommand request)
//    {
//        await _mediator.Send(request);
//        return Ok();
//    }
//}
