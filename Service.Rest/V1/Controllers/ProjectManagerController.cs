using Application.Query.Queries;
using Application.Query.ViewModels.Tenants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.IdentityServerProvider.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/tenant-panel/project-managers")]
    [ApiController]
    [Authorize]
    public class ProjectManagerController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;
        public ProjectManagerController(IMediator mediator, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
        }


        [HttpGet]
        [ActionName(nameof(GetByTenantIdAsync))]
        [SwaggerOperation("get project manager by tenant id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "project manager returned", typeof(ProjectManagerViewModel))]
        public async Task<ActionResult<ProjectManagerViewModel>> GetByTenantIdAsync()
        {
            var result = await _mediator.Send(new GetAllProjectManagerQuery(_currentUserService.TenantId));
            return Ok(result);
        }


    }

}