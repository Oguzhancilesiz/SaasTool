using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaasTool.DTO.Apps;
using SaasTool.DTO.Common;
using SaasTool.Service.Abstracts;

namespace SaasTool.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/features")]
[Authorize] // login şart
public class FeaturesController : ControllerBase
{
    private readonly IFeatureService _svc;
    public FeaturesController(IFeatureService svc) { _svc = svc; }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] FeatureCreateDto dto, CancellationToken ct)
        => Ok(await _svc.CreateAsync(dto, ct));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] FeatureUpdateDto dto, CancellationToken ct)
    {
        await _svc.UpdateAsync(id, dto, ct);
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FeatureDto>> Get(Guid id, CancellationToken ct)
    {
        var data = await _svc.GetAsync(id, ct);
        return data is null ? NotFound() : Ok(data);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<FeatureDto>>> List([FromQuery] Guid? appId, [FromQuery] PagedRequest req, CancellationToken ct)
        => Ok(await _svc.ListAsync(appId, req, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _svc.DeleteAsync(id, ct);
        return NoContent();
    }
}
