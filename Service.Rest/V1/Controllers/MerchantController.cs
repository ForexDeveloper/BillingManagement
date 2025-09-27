using Application.Query.Queries.Merchants;
using Application.Query.ViewModels.Merchants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.IdentityServerProvider.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/tenant-panel/merchants")]
    [ApiController]
    [Authorize]
    public class MerchantController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;

        public MerchantController(IMediator mediator, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        [SwaggerOperation("get List  merchant ")]
        [SwaggerResponse((int)HttpStatusCode.OK, "merchant returned", typeof(GetMerchantListVm))]
        public async Task<ActionResult<GetMerchantListVm>> GetListAsync()
        => Ok(await _mediator.Send(new GetMerchantListQuery(_currentUserService.TenantId)));


        [HttpGet("{branchTerminalId}/validate")]
        [SwaggerOperation("Validate merchant branch")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Merchant branch validation returned", typeof(bool))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Merchant branch not found")]
        public async Task<ActionResult<bool>> ValidateMerchantBranch([FromRoute] long branchTerminalId)
        {
            return Ok(await _mediator.Send(new ValidateMerchantBranchQuery(branchTerminalId)));
        }

        [HttpGet("{id}/branches")]
        [ActionName(nameof(GetMerchantBranchesAsync))]
        [SwaggerOperation("Get a merchant branch by id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "merchant branch returned", typeof(GetMerchantBranchVM))]
        public async Task<ActionResult<List<GetMerchantBranchVM>>> GetMerchantBranchesAsync([FromRoute] int id)
        {
            var result = await _mediator.Send(new GetMerchantBranchesByTenantIdQuery(id, _currentUserService.TenantId));
            return Ok(result);
        }

        [HttpGet("{id:int}/active-contract-exists")]
        [SwaggerOperation("Has tenant merchant contract")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Has tenant merchant contractn returned", typeof(bool))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Tenant merchant contractn not found")]
        public async Task<ActionResult<bool>> HasActiveTenantMerchantContract([FromRoute] int id)
        {
            return Ok(await _mediator.Send(new HasActiveTenantMerchantContractByMerchantIdQuery(id, _currentUserService.TenantId)));
        }
    }
}
