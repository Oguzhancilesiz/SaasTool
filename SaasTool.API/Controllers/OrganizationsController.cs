using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaasTool.DTO.Common;
using SaasTool.DTO.Orgs;
using SaasTool.Service.Abstracts;

namespace SaasTool.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/organizations")]
    [Authorize]
    public class OrganizationsController : ControllerBase
    {
        private readonly IOrganizationService _svc;
        public OrganizationsController(IOrganizationService svc) { _svc = svc; }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] OrganizationCreateDto dto, CancellationToken ct)
            => Ok(await _svc.CreateAsync(dto, ct));

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] OrganizationUpdateDto dto, CancellationToken ct)
        { await _svc.UpdateAsync(id, dto, ct); return NoContent(); }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<OrganizationDto>> Get(Guid id, CancellationToken ct)
            => (await _svc.GetAsync(id, ct)) is { } d ? Ok(d) : NotFound();

        [HttpGet]
        public async Task<ActionResult<PagedResponse<OrganizationDto>>> List([FromQuery] PagedRequest req, CancellationToken ct)
            => Ok(await _svc.ListAsync(req, ct));

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        { await _svc.DeleteAsync(id, ct); return NoContent(); }
    }
}
