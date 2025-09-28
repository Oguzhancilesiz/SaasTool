using SaasTool.DTO.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Service.Abstracts
{
    public interface IDashboardService
    {
        Task<KpiDto> GetKpisAsync(Guid orgId, Guid? appId, CancellationToken ct);
        Task<IReadOnlyList<SeriesPointDto>> GetRevenueSeriesAsync(Guid orgId, Guid? appId, DateTime fromUtc, DateTime toUtc, CancellationToken ct);
        Task<IReadOnlyList<SeriesPointDto>> GetSubscriptionSeriesAsync(Guid orgId, Guid? appId, DateTime fromUtc, DateTime toUtc, CancellationToken ct);
        Task<IReadOnlyList<BreakdownItemDto>> GetPlanBreakdownAsync(Guid orgId, Guid? appId, DateTime fromUtc, DateTime toUtc, CancellationToken ct);
        Task<FunnelDto> GetFunnelAsync(Guid orgId, Guid? appId, DateTime fromUtc, DateTime toUtc, CancellationToken ct);
        Task<IReadOnlyList<RecentEventDto>> GetRecentEventsAsync(Guid orgId, int take, CancellationToken ct);
        Task<IReadOnlyList<HealthDto>> GetHealthAsync(CancellationToken ct);
        Task<IReadOnlyList<TopCustomerDto>> GetTopCustomersAsync(Guid orgId, DateTime fromUtc, DateTime toUtc, int take, CancellationToken ct);

    }

}
