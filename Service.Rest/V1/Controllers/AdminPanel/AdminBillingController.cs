using MediatR;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Application.Query.Queries.Billings;
using Swashbuckle.AspNetCore.Annotations;
using Application.Query.ViewModels.Billings;
using Service.Rest.V1.RequestModels.Billings;
using Shared.IdentityServerProvider.Contracts;

namespace Service.Rest.V1.Controllers.AdminPanel;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/admin-panel/billings/tenants")]
public sealed class AdminBillingController(IMediator mediator, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet("{id}")]
    [SwaggerOperation("Get billing list")]
    [SwaggerResponse((int)HttpStatusCode.NotFound, "billing list not found")]
    [SwaggerResponse((int)HttpStatusCode.OK, "billing list returned", typeof(GetBillingsVm))]
    public async Task<ActionResult> GetListAsync([FromRoute] int id, [FromQuery] GetBillingsRequest request)
    {
        var billings = await mediator.Send(new GetBillingsQuery(id, request.MerchantId, request.Code, request.Type,
            request.Status, request.StartDate, request.DueDate, request.PageSize, request.PageIndex));

        return Ok(billings);
    }
}