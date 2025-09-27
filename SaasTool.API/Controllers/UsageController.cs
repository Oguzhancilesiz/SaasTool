using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaasTool.DTO.Billing;
using SaasTool.DTO.Common;
using SaasTool.Service.Abstracts;

namespace SaasTool.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/usage")]
    [Authorize]
    public class UsageController : ControllerBase
    {
        private readonly IUsageService _svc;
        public UsageController(IUsageService svc) { _svc = svc; }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] UsageRecordCreateDto dto, CancellationToken ct)
            => Ok(await _svc.CreateAsync(dto, ct));

        [HttpGet]
        public async Task<ActionResult<PagedResponse<UsageRecordDto>>> List([FromQuery] Guid? subscriptionId, [FromQuery] Guid? featureId, [FromQuery] PagedRequest req, CancellationToken ct)
            => Ok(await _svc.ListAsync(subscriptionId, featureId, req, ct));
    }
}
