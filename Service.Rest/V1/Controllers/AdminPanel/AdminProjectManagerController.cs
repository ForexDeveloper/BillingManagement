using Application.Query.Queries;
using Application.Query.ViewModels.Tenants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers.AdminPanel
{
    [ApiVersion("1.0")]
    [Route("api/admin-panel/project-managers")]
    [ApiController]
    [Authorize]
    public class AdminProjectManagerController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminProjectManagerController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpGet]
        [Route("{tenantId}")]
        [ActionName(nameof(GetByTenantIdAsync))]
        [SwaggerOperation("get project manager by tenant id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "project manager returned", typeof(ProjectManagerViewModel))]
        public async Task<ActionResult<ProjectManagerViewModel>> GetByTenantIdAsync([FromRoute] int tenantId)
        {
            var result = await _mediator.Send(new GetAllProjectManagerQuery(tenantId));
            return Ok(result);
        }


    }

}