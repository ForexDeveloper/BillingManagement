using MediatR;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Application.Query.Queries.Billings;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;
using Application.Query.ViewModels.Billings;
using Service.Rest.V1.RequestModels.Billings;
using Shared.IdentityServerProvider.Contracts;

namespace Service.Rest.V1.Controllers;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/tenant-panel/billings")]
public sealed class BillingController(IMediator mediator, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation("Get billing list")]
    [SwaggerResponse((int)HttpStatusCode.NotFound, "billing list not found")]
    [SwaggerResponse((int)HttpStatusCode.OK, "billing list returned", typeof(GetBillingsViewModel))]
    public async Task<ActionResult> GetListAsync([FromQuery] GetBillingsRequest request)
    {
        var billings = await mediator.Send(new GetBillingsQuery(currentUserService.TenantId, request.MerchantId,
            request.Code, request.Type, request.Status, request.PageSize, request.PageIndex));

        return Ok(billings);
    }
}