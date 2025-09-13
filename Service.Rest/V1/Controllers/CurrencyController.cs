using Application.Query.Queries;
using Application.Query.ViewModels;
using Application.Query.ViewModels.Currencies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/currencies")]
    [ApiController]
    [Authorize]
    public class CurrencyController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CurrencyController(IMediator mediator)
        {
            _mediator = mediator;
        }
        

        [HttpGet]
        [ActionName(nameof(GetAllAsync))]
        [SwaggerOperation("get currency list")]
        [SwaggerResponse((int)HttpStatusCode.OK, "currency returned", typeof(CurrencyViewModel))]
        public async Task<ActionResult<CurrencyViewModel>> GetAllAsync()
        {
            var result = await _mediator.Send(new GetAllCurrencyQuery());
            return Ok(result);
        }


    }

}