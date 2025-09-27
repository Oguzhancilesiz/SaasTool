using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaasTool.DTO.Common;
using SaasTool.DTO.Plans;
using SaasTool.Service.Abstracts;

namespace SaasTool.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/plan-features")]
    [Authorize]
    public class PlanFeaturesController : ControllerBase
    {
        private readonly IPlanFeatureService _svc;
        public PlanFeaturesController(IPlanFeatureService svc) { _svc = svc; }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] PlanFeatureCreateDto dto, CancellationToken ct)
            => Ok(await _svc.CreateAsync(dto, ct));

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PlanFeatureUpdateDto dto, CancellationToken ct)
        { await _svc.UpdateAsync(id, dto, ct); return NoContent(); }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<PlanFeatureDto>> Get(Guid id, CancellationToken ct)
            => (await _svc.GetAsync(id, ct)) is { } d ? Ok(d) : NotFound();

        [HttpGet]
        public async Task<ActionResult<PagedResponse<PlanFeatureDto>>> List([FromQuery] Guid? planId, [FromQuery] PagedRequest req, CancellationToken ct)
            => Ok(await _svc.ListAsync(planId, req, ct));

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        { await _svc.DeleteAsync(id, ct); return NoContent(); }
    }
}
