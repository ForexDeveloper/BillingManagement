using Application.Query.Queries;
using Application.Query.ViewModels.Financiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Rest.V1.RequestModels.Financiers;
using Shared.IdentityServerProvider.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/tenant-panel/financiers")]
    [ApiController]
    [Authorize]
    public class FinancierController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;
        public FinancierController(IMediator mediator, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetAsync))]
        [SwaggerOperation("Get a tenant financier by id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Tenant financier returned", typeof(GetFinancierVm))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Tenant financier not found")]
        public async Task<ActionResult<GetFinancierVm>> GetAsync([FromRoute] int id)
        {
            var result = await _mediator.Send(new GetFinancierByIdQuery(id, _currentUserService.TenantId));
            return Ok(result);
        }

        [HttpGet]
        [SwaggerOperation("Get tenant financiers list")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Tenant financiers list returned", typeof(GetFinanciersVm))]
        public async Task<ActionResult<GetFinanciersVm>> GetListAsync([FromQuery] GetTenantFinanciersModel request)
        {
            var tenants = await _mediator.Send(new GetFinanciersQuery(_currentUserService.TenantId, request.PageIndex, request.PageSize, request.SortColumn,
                request.SortDirection, request.SearchValue, request.PersonType));

            return Ok(tenants);
        }
    }
}