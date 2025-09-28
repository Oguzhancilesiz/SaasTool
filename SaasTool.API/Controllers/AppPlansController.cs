using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaasTool.DTO.Apps;
using SaasTool.DTO.Common;
using SaasTool.Service.Abstracts;

namespace SaasTool.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/app-plans")]
    [Authorize] // gerekirse rol/policy ekleyebilirsin
    public sealed class AppPlansController(IAppPlanService svc) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] AppPlanCreateDto dto, CancellationToken ct)
        {
            var id = await svc.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id, version = "1.0" }, id);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] AppPlanUpdateDto dto, CancellationToken ct)
        {
            await svc.UpdateAsync(id, dto, ct);
            return NoContent();
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<AppPlanDto>> GetById(Guid id, CancellationToken ct)
        {
            var res = await svc.GetAsync(id, ct);
            return res is null ? NotFound() : Ok(res);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<AppPlanDto>>> List(
            [FromQuery] Guid? appId,
            [FromQuery] Guid? planId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var req = new PagedRequest { Page = page, PageSize = pageSize, Search = null };
            var res = await svc.ListAsync(appId, planId, req, ct);
            return Ok(res);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await svc.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}
