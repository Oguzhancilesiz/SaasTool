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
    [Route("api/v{version:apiVersion}/apps")]
    [Authorize]
    public class AppsController : ControllerBase
    {
        private readonly IAppService _svc;
        public AppsController(IAppService svc) { _svc = svc; }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] AppCreateDto dto, CancellationToken ct)
            => Ok(await _svc.CreateAsync(dto, ct));

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] AppUpdateDto dto, CancellationToken ct)
        { await _svc.UpdateAsync(id, dto, ct); return NoContent(); }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<AppDto>> Get(Guid id, CancellationToken ct)
            => (await _svc.GetAsync(id, ct)) is { } d ? Ok(d) : NotFound();

        [HttpGet]
        public async Task<ActionResult<PagedResponse<AppDto>>> List([FromQuery] PagedRequest req, CancellationToken ct)
            => Ok(await _svc.ListAsync(req, ct));

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        { await _svc.DeleteAsync(id, ct); return NoContent(); }
    }
}
