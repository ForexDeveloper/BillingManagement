using Application.Query.Queries;
using Application.Query.ViewModels.Categories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/categories")]
    [ApiController]
    [Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly IMediator _mediator;
        public CategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [SwaggerOperation("get List  category ")]
        [SwaggerResponse((int)HttpStatusCode.OK, "category returned", typeof(GetCategoryListVm))]
        public async Task<ActionResult<GetCategoryListVm>> GetListAsync()
        => Ok(await _mediator.Send(new GetCategoryListQuery()));

    }
}
