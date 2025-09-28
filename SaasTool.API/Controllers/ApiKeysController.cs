using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaasTool.DTO.Common;
using SaasTool.DTO.Security;
using SaasTool.Service.Abstracts;

namespace SaasTool.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/api-keys")]
    [Authorize]
    public sealed class ApiKeysController : ControllerBase
    {
        private readonly IApiKeyService _svc;
        public ApiKeysController(IApiKeyService svc) { _svc = svc; }

        [HttpPost]
        [ProducesResponseType(typeof(ApiKeyCreatedDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<ApiKeyCreatedDto>> Create([FromBody] ApiKeyCreateDto dto, CancellationToken ct)
        {
            var res = await _svc.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(List), new { version = "1.0" }, res);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<ApiKeyDto>>> List([FromQuery] Guid? organizationId, [FromQuery] Guid? appId, [FromQuery] PagedRequest req, CancellationToken ct)
            => Ok(await _svc.ListAsync(organizationId, appId, req, ct));

        [HttpPost("{id:guid}/revoke")]
        public async Task<IActionResult> Revoke(Guid id, CancellationToken ct)
        { await _svc.RevokeAsync(id, ct); return NoContent(); }
    }
}
