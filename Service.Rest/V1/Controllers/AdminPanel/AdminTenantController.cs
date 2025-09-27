using Application.Query.Queries.Tenants;
using Application.Query.ViewModels.Tenants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Rest.V1.RequestModels.Tenants;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers.AdminPanel
{
    [ApiVersion("1.0")]
    [Route("api/admin-panel/tenants")]
    [ApiController]
    [Authorize]
    public class AdminTenantController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminTenantController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [SwaggerOperation("Get tenant list")]
        [SwaggerResponse((int)HttpStatusCode.OK, "tenant list returned", typeof(GetTenantsVm))]
        public async Task<ActionResult<GetTenantsVm>> GetListAsync([FromQuery] GetTenantsModel request)
        {
            var tenants = await _mediator.Send(new
                GetTenantsQuery(request.PageIndex,
                request.PageSize, request.SortColumn,
                request.SortDirection,
                request.SearchValue));

            return Ok(tenants);
        }
    }

}