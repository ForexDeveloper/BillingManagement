using Application.Query.Queries;
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
    [Route("api/admin-panel")]
    [ApiController]
    [Authorize]
    public class AdminFinancialDocumentController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AdminFinancialDocumentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("financial-documents/purchases")]
        [SwaggerOperation("Get tenant purchases list")]
        [SwaggerResponse((int)HttpStatusCode.OK, "tenant purchases list returned", typeof(GetPurchasesListVm))]
        public async Task<ActionResult<GetPurchasesListVm>> GetPurchasesList([FromQuery] GetAdminPurchasesListFilterModel filter, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetPurchasesListQuery
            {
                PageIndex = filter.PageIndex,
                MerchantIds = filter.MerchantIds,
                StartDate = filter.FromDate,
                EndDate = filter.ToDate,
                TenantId = filter.TenantId,
                SearchValue = filter.SearchValue
            }, cancellationToken);
            return Ok(result);
        }
        [HttpGet("financial-documents/purchases/{id}")]
        [SwaggerOperation("Get tenant purchases list")]
        [SwaggerResponse((int)HttpStatusCode.OK, "tenant purchases list returned", typeof(GetTenantsVm))]
        public async Task<ActionResult<GetPurchaseVm>> GetPurchase([FromRoute] long id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetPurchaseQuery
            {
                FinancialDocumentId = id,
            }, cancellationToken);
            return Ok(result);
        }
    }
}
