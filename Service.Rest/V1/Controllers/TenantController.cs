using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Service.Rest.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/tenant-panel/tenants")]
    [ApiController]
    [Authorize]
    public class TenantController : ControllerBase
    {
    }
}
