using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaasTool.DTO.Dashboard;
using SaasTool.Service.Abstracts;

namespace SaasTool.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/dashboard")]
    [Authorize] // Policy: Dashboard.View / Dashboard.Finance gibi ayrıştırabilirsin
    public sealed class DashboardController : ControllerBase
    {
        private readonly IDashboardService _svc;
        public DashboardController(IDashboardService svc) { _svc = svc; }

        [HttpGet("kpis")]
        public async Task<ActionResult<KpiDto>> GetKpis([FromQuery] Guid orgId, [FromQuery] Guid? appId, CancellationToken ct)
            => Ok(await _svc.GetKpisAsync(orgId, appId, ct));

        [HttpGet("timeseries/revenue")]
        public async Task<ActionResult<IReadOnlyList<SeriesPointDto>>> RevenueSeries(
            [FromQuery] Guid orgId,
            [FromQuery] Guid? appId,
            [FromQuery] DateTime fromUtc,
            [FromQuery] DateTime toUtc,
            CancellationToken ct)
            => Ok(await _svc.GetRevenueSeriesAsync(orgId, appId, fromUtc, toUtc, ct));

        [HttpGet("timeseries/subscriptions")]
        public async Task<ActionResult<IReadOnlyList<SeriesPointDto>>> SubscriptionSeries(
            [FromQuery] Guid orgId,
            [FromQuery] Guid? appId,
            [FromQuery] DateTime fromUtc,
            [FromQuery] DateTime toUtc,
            CancellationToken ct)
            => Ok(await _svc.GetSubscriptionSeriesAsync(orgId, appId, fromUtc, toUtc, ct));

        [HttpGet("breakdown/plans")]
        public async Task<ActionResult<IReadOnlyList<BreakdownItemDto>>> PlanBreakdown(
            [FromQuery] Guid orgId,
            [FromQuery] Guid? appId,
            [FromQuery] DateTime fromUtc,
            [FromQuery] DateTime toUtc,
            CancellationToken ct)
            => Ok(await _svc.GetPlanBreakdownAsync(orgId, appId, fromUtc, toUtc, ct));

        [HttpGet("funnel")]
        public async Task<ActionResult<FunnelDto>> Funnel(
            [FromQuery] Guid orgId,
            [FromQuery] Guid? appId,
            [FromQuery] DateTime fromUtc,
            [FromQuery] DateTime toUtc,
            CancellationToken ct)
            => Ok(await _svc.GetFunnelAsync(orgId, appId, fromUtc, toUtc, ct));

        [HttpGet("events/recent")]
        public async Task<ActionResult<IReadOnlyList<RecentEventDto>>> RecentEvents([FromQuery] int take = 20, CancellationToken ct = default)
            => Ok(await _svc.GetRecentEventsAsync(orgId: Guid.Empty /* org yoksa */, take, ct));
        // İstersen orgId zorunlu yap: [FromQuery] Guid orgId ekle ve servise geçir.

        [HttpGet("health")]
        [AllowAnonymous] // status sayfasına açmak isteyebilirsin
        public async Task<ActionResult<IReadOnlyList<HealthDto>>> Health(CancellationToken ct)
            => Ok(await _svc.GetHealthAsync(ct));

        [HttpGet("top-customers")]
        public async Task<ActionResult<IEnumerable<TopCustomerDto>>> TopCustomers([FromQuery] Guid orgId,
    [FromQuery] DateTime fromUtc, [FromQuery] DateTime toUtc, [FromQuery] int take = 5, CancellationToken ct = default)
    => Ok(await _svc.GetTopCustomersAsync(orgId, fromUtc, toUtc, take, ct));

    }
}
