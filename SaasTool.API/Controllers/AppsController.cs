using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaasTool.API.Infrastructure.Extensions;
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
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        public async Task<ActionResult<Guid>> Create([FromBody] AppCreateDto dto, CancellationToken ct)
        {
            var id = await _svc.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(Get), new { id, version = "1.0" }, id);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Update(Guid id, [FromBody] AppUpdateDto dto, CancellationToken ct)
        { await _svc.UpdateAsync(id, dto, ct); return NoContent(); }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(AppDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AppDto>> Get(Guid id, CancellationToken ct)
            => (await _svc.GetAsync(id, ct)) is { } d ? Ok(d) : NotFound();

        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<AppDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResponse<AppDto>>> List([FromQuery] PagedRequest req, CancellationToken ct)
        {
            var n = req.Normalize();
            var res = await _svc.ListAsync(n, ct);
            var etag = $"W/\"apps:{res.Total}:{n.Page}:{n.PageSize}:{n.Search}\"";
            if (Request.TryShortCircuitWithEtag(Response, etag)) return StatusCode(StatusCodes.Status304NotModified);
            return Ok(res);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        { await _svc.DeleteAsync(id, ct); return NoContent(); }
    }
}
