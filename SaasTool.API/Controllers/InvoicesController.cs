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
    [Route("api/v{version:apiVersion}/invoices")]
    [Authorize]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _svc;
        public InvoicesController(IInvoiceService svc) { _svc = svc; }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] InvoiceCreateDto dto, CancellationToken ct)
            => Ok(await _svc.CreateAsync(dto, ct));

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<InvoiceDto>> Get(Guid id, CancellationToken ct)
            => (await _svc.GetAsync(id, ct)) is { } d ? Ok(d) : NotFound();

        [HttpGet]
        public async Task<ActionResult<PagedResponse<InvoiceDto>>> List([FromQuery] Guid? organizationId, [FromQuery] PagedRequest req, CancellationToken ct)
            => Ok(await _svc.ListAsync(organizationId, req, ct));
    }
}
