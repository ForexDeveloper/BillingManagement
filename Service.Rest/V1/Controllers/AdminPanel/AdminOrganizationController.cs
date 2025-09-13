using Application.Query.Queries;
using Application.Query.ViewModels.Organizations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Rest.V1.RequestModels.Organizations;
using Shared.IdentityServerProvider.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers.AdminPanel
{
    [ApiVersion("1.0")]
    [Route("api/admin-panel/organizations")]
    [ApiController]
    [Authorize]
    public class AdminOrganizationController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;

        public AdminOrganizationController(IMediator mediator, ICurrentUserService currentUserService)
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
            var organizationModel = await _mediator.Send(new GetOrganizationByIdQuery(id));
            return Ok(organizationModel);
        }

        [HttpGet]
        [SwaggerOperation("Get organizations list")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Organizations list returned", typeof(GetOrganizationsVm))]
        public async Task<ActionResult<GetOrganizationsVm>> GetListAsync([FromQuery] GetOrganizationsModel request)
        {
            var tenants = await _mediator.Send(new GetOrganizationsQuery(request.TenantId, request.PageIndex, request.PageSize, request.SortColumn,
                request.SortDirection, request.SearchValue));

            return Ok(tenants);
        }

        [HttpGet("{tenantId}/tree")]
        [SwaggerOperation("Get organizations list as a tree")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Organizations list returned", typeof(GetOrganizationsTreeVm))]
        public async Task<ActionResult<GetOrganizationsTreeVm>> GetTreeAsync([FromRoute] int tenantId)
        {
            var organizations = await _mediator.Send(new GetOrganizationsByTenantIdQuery(tenantId));
            return Ok(organizations);
        }
    }
}