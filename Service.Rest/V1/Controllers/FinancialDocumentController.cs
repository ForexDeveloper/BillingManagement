using Application.Query.Queries;
using Application.Query.ViewModels.Tenants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Rest.V1.RequestModels.Tenants;
using Shared.IdentityServerProvider.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/tenant-panel")]
    [ApiController]
    [Authorize]
    public class FinancialDocumentController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;
        public FinancialDocumentController(IMediator mediator, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
        }

        [HttpGet("financial-documents/purchases")]
        [SwaggerOperation("Get tenant purchases list")]
        [SwaggerResponse((int)HttpStatusCode.OK, "tenant purchases list returned", typeof(GetPurchasesListVm))]
        public async Task<ActionResult<GetPurchasesListVm>> GetPurchasesList(
            [FromQuery] GetTenantPurchasesListFilterModel filter,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetPurchasesListQuery
            {
                PageIndex = filter.PageIndex,
                MerchantIds = filter.MerchantIds,
                StartDate = filter.FromDate,
                EndDate = filter.ToDate,
                TenantId = _currentUserService.TenantId,
                SearchValue = filter.SearchValue
            }, cancellationToken);
            return Ok(result);
        }


        [HttpGet("financial-documents/purchases/{id}")]
        [SwaggerOperation("Get tenant purchase")]
        [SwaggerResponse((int)HttpStatusCode.OK, "tenant purchase", typeof(GetTenantsVm))]
        public async Task<ActionResult<GetPurchasesListVm>> GetPurchase([FromRoute] long id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetPurchaseQuery
            {
                FinancialDocumentId = id,
            }, cancellationToken);
            return Ok(result);
        }
    }
}
