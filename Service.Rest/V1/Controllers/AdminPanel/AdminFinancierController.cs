using Application.Query.Queries.Financiers;
using Application.Query.ViewModels.Financiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Rest.V1.RequestModels.Financiers;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers.AdminPanel
{
    [ApiVersion("1.0")]
    [Route("api/admin-panel/financiers")]
    [ApiController]
    [Authorize]
    public class AdminFinancierController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AdminFinancierController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetAsync))]
        [SwaggerOperation("Get a financier by id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Financier returned", typeof(GetFinancierVm))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Financier not found")]
        public async Task<ActionResult<GetFinancierVm>> GetAsync([FromRoute] int id)
        {
            var result = await _mediator.Send(new GetFinancierByIdQuery(id));
            return Ok(result);
        }

        [HttpGet]
        [SwaggerOperation("Get financiers list")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Financiers list returned", typeof(GetFinanciersVm))]
        public async Task<ActionResult<GetFinanciersVm>> GetListAsync([FromQuery] GetFinanciersModel request)
        {
            var tenants = await _mediator.Send(new GetFinanciersQuery(request.TenantId, request.PageIndex, request.PageSize, request.SortColumn,
                request.SortDirection, request.SearchValue, request.PersonType));

            return Ok(tenants);
        }
    }
}