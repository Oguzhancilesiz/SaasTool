using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using SaasTool.DTO.Common;
using SaasTool.DTO.Plans;
using SaasTool.Service.Abstracts;

namespace SaasTool.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/plans")]
    public class PlansController : ControllerBase
    {
        private readonly IPlanService _service;
        public PlansController(IPlanService service) { _service = service; }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] PlanCreateDto dto, CancellationToken ct)
            => Ok(await _service.CreateAsync(dto, ct));

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PlanUpdateDto dto, CancellationToken ct)
        {
            await _service.UpdateAsync(id, dto, ct);
            return NoContent();
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<PlanDto>> Get(Guid id, CancellationToken ct)
        {
            var data = await _service.GetAsync(id, ct);
            if (data is null) return NotFound();
            return Ok(data);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<PlanDto>>> List([FromQuery] PagedRequest req, CancellationToken ct)
            => Ok(await _service.ListAsync(req, ct));

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}
