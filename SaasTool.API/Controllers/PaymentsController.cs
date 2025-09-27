using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaasTool.DTO.Billing;
using SaasTool.Service.Abstracts;

namespace SaasTool.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/payments")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _svc;
        public PaymentsController(IPaymentService svc) { _svc = svc; }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] PaymentCreateDto dto, CancellationToken ct)
            => Ok(await _svc.CreateAsync(dto, ct));
    }
}
