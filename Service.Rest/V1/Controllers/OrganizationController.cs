using Application.Query.Queries.Organizations;
using Application.Query.ViewModels.Organizations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Rest.V1.RequestModels.Organizations;
using Shared.IdentityServerProvider.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/tenant-panel/organizations")]
    [ApiController]
    [Authorize]
    public class OrganizationController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;
        public OrganizationController(IMediator mediator, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetAsync))]
        [SwaggerOperation("Get an organization by id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Organization returned", typeof(GetOrganizationVm))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Organization not found")]
        public async Task<ActionResult<GetOrganizationVm>> GetAsync([FromRoute] int id)
        {
            var organizationModel = await _mediator.Send(new GetOrganizationByIdQuery(id, _currentUserService.TenantId));
            return Ok(organizationModel);
        }

        [HttpGet]
        [SwaggerOperation("Get organizations list")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Organizations list returned", typeof(GetOrganizationsVm))]
        public async Task<ActionResult<GetOrganizationsVm>> GetListAsync([FromQuery] GetOrganizationsBaseModel request)
        {
            var tenants = await _mediator.Send(new GetOrganizationsQuery(_currentUserService.TenantId, request.PageIndex, request.PageSize, request.SortColumn,
                request.SortDirection, request.SearchValue));

            return Ok(tenants);
        }

        [HttpGet("tree")]
        [SwaggerOperation("Get tenant organizations list as a tree")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Organizations list returned", typeof(GetOrganizationsTreeVm))]
        public async Task<ActionResult<GetOrganizationsTreeVm>> GetTreeAsync()
        {
            var organizations = await _mediator.Send(new GetOrganizationsByTenantIdQuery(_currentUserService.TenantId));
            return Ok(organizations);
        }
    }
}