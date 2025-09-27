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
    [Route("api/v{version:apiVersion}/subscriptions")]
    [Authorize]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ISubscriptionService _svc;
        public SubscriptionsController(ISubscriptionService svc) { _svc = svc; }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] SubscriptionCreateDto dto, CancellationToken ct)
            => Ok(await _svc.CreateAsync(dto, ct));

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<SubscriptionDto>> Get(Guid id, CancellationToken ct)
            => (await _svc.GetAsync(id, ct)) is { } d ? Ok(d) : NotFound();

        [HttpGet]
        public async Task<ActionResult<PagedResponse<SubscriptionDto>>> List([FromQuery] Guid? organizationId, [FromQuery] PagedRequest req, CancellationToken ct)
            => Ok(await _svc.ListAsync(organizationId, req, ct));
    }
}
