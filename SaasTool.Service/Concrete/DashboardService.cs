using Microsoft.EntityFrameworkCore;
using SaasTool.Core.Abstracts;
using SaasTool.Core.Enums;
using SaasTool.DTO.Dashboard;
using SaasTool.Entity;
using SaasTool.Service.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Service.Concrete
{
    public sealed class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _uow;

        public DashboardService(IUnitOfWork uow) { _uow = uow; }

        public async Task<KpiDto> GetKpisAsync(Guid orgId, Guid? appId, CancellationToken ct)
        {
            var now = DateTime.UtcNow;
            var todayStart = now.Date;
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1);

            // Payments
            var payQ = (await _uow.Repository<Payment>().GetAllActives())
                .AsNoTracking()
                .Where(p => p.Invoice.OrganizationId == orgId);

            var revenueToday = await payQ
                .Where(p => p.PaidAt >= todayStart)
                .SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;

            var revenueMtd = await payQ
                .Where(p => p.PaidAt >= monthStart && p.PaidAt < monthEnd)
                .SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;

            // Customers (son 7 gün)
            var newCust7d = await (await _uow.Repository<Customer>().GetAllActives())
                .AsNoTracking()
                .Where(c => c.OrganizationId == orgId && c.CreatedDate >= now.AddDays(-7))
                .CountAsync(ct);

            // Subscriptions (app filtresi dahil)
            var subsQ = (await _uow.Repository<Subscription>().GetAllActives())
                .AsNoTracking()
                .Where(s => s.OrganizationId == orgId);

            if (appId is not null)
                subsQ = subsQ.Where(s => s.AppId == appId);

            var activeSubscriptions = await subsQ
                .Where(s =>
                    (s.SubscriptionState == SubscriptionStatus.Active ||
                     (s.TrialEndsAt != null && s.TrialEndsAt > now)) &&
                    (s.EndsAt == null || s.EndsAt > now))
                .CountAsync(ct);

            // MRR: gerekli alanları çek -> C# tarafında normalize et
            var mrrPlans = await subsQ
                .Where(s => s.SubscriptionState == SubscriptionStatus.Active && (s.EndsAt == null || s.EndsAt > now))
                .Select(s => new { s.Plan.Price, s.Plan.BillingPeriod })
                .ToListAsync(ct);

            decimal mrr = 0m;
            foreach (var p in mrrPlans)
                mrr += NormalizeMonthly(p.Price, p.BillingPeriod);

            // Churn: ay başında aktif olanlar
            var activeAtMonthStart = await subsQ
                .Where(s =>
                    (s.SubscriptionState == SubscriptionStatus.Active ||
                     (s.TrialEndsAt != null && s.TrialEndsAt > monthStart)) &&
                    s.StartsAt <= monthStart &&
                    (s.EndsAt == null || s.EndsAt > monthStart))
                .CountAsync(ct);

            var churnedThisMonth = await subsQ
                .Where(s => s.EndsAt != null && s.EndsAt >= monthStart && s.EndsAt < monthEnd)
                .CountAsync(ct);

            var churnRate = activeAtMonthStart > 0
                ? (decimal)churnedThisMonth / activeAtMonthStart
                : 0m;

            return new KpiDto
            {
                RevenueToday = revenueToday,
                RevenueMTD = revenueMtd,
                NewCustomers7d = newCust7d,
                ActiveSubscriptions = activeSubscriptions,
                Mrr = mrr,
                ChurnRate = DecimalRound(churnRate, 4)
            };
        }

        public async Task<IReadOnlyList<SeriesPointDto>> GetRevenueSeriesAsync(Guid orgId, Guid? appId, DateTime fromUtc, DateTime toUtc, CancellationToken ct)
        {
            var q = (await _uow.Repository<Payment>().GetAllActives())
                .AsNoTracking()
                .Where(p => p.Invoice.OrganizationId == orgId && p.PaidAt >= fromUtc && p.PaidAt <= toUtc);

            // appId filtresi: InvoiceLine -> Subscription/Feature navigation yoksa uygulanamaz
            // TODO: InvoiceLine'a Subscription ve Feature navigasyonları eklendiğinde burayı aç.
            // if (appId is not null) q = q.Where(p => p.Invoice.Lines.Any(l => ... ));

            var list = await q
                .GroupBy(p => p.PaidAt.Date)
                .Select(g => new { T = g.Key, V = g.Sum(x => x.Amount) })
                .OrderBy(x => x.T)
                .ToListAsync(ct);

            return list.Select(x => new SeriesPointDto { T = DateTime.SpecifyKind(x.T, DateTimeKind.Utc), V = x.V }).ToList();
        }

        public async Task<IReadOnlyList<SeriesPointDto>> GetSubscriptionSeriesAsync(Guid orgId, Guid? appId, DateTime fromUtc, DateTime toUtc, CancellationToken ct)
        {
            var q = (await _uow.Repository<Subscription>().GetAllActives())
                .AsNoTracking()
                .Where(s => s.OrganizationId == orgId && s.CreatedDate >= fromUtc && s.CreatedDate <= toUtc);
            if (appId is not null) q = q.Where(s => s.AppId == appId);

            var list = await q
                .GroupBy(s => s.CreatedDate.Date)
                .Select(g => new { T = g.Key, V = g.Count() })
                .OrderBy(x => x.T)
                .ToListAsync(ct);

            return list.Select(x => new SeriesPointDto { T = DateTime.SpecifyKind(x.T, DateTimeKind.Utc), V = x.V }).ToList();
        }

        public async Task<IReadOnlyList<BreakdownItemDto>> GetPlanBreakdownAsync(Guid orgId, Guid? appId, DateTime fromUtc, DateTime toUtc, CancellationToken ct)
        {
            var q = (await _uow.Repository<Subscription>().GetAllActives())
                .AsNoTracking()
                .Include(s => s.Plan)
                .Where(s => s.OrganizationId == orgId);

            if (appId is not null) q = q.Where(s => s.AppId == appId);
            if (fromUtc != default || toUtc != default)
            {
                // aktiflik aralığına düşenler
                q = q.Where(s => (s.StartsAt <= toUtc) && (s.EndsAt == null || s.EndsAt >= fromUtc));
            }

            var list = await q
                .GroupBy(s => s.Plan.Name)
                .Select(g => new BreakdownItemDto { Key = g.Key, Value = g.Count() })
                .OrderByDescending(x => x.Value)
                .ToListAsync(ct);

            return list;
        }

        public async Task<FunnelDto> GetFunnelAsync(Guid orgId, Guid? appId, DateTime fromUtc, DateTime toUtc, CancellationToken ct)
        {
            var customersQ = (await _uow.Repository<Customer>().GetAllActives())
                .AsNoTracking()
                .Where(c => c.OrganizationId == orgId && c.CreatedDate >= fromUtc && c.CreatedDate <= toUtc);

            var subsQ = (await _uow.Repository<Subscription>().GetAllActives())
                .AsNoTracking()
                .Where(s => s.OrganizationId == orgId && s.CreatedDate >= fromUtc && s.CreatedDate <= toUtc);

            if (appId is not null)
                subsQ = subsQ.Where(s => s.AppId == appId);

            var now = DateTime.UtcNow;

            var signedUp = await customersQ.CountAsync(ct);
            var trial = await subsQ.Where(s => s.TrialEndsAt != null && s.TrialEndsAt > now).CountAsync(ct);
            var active = await subsQ.Where(s => s.SubscriptionState == SubscriptionStatus.Active).CountAsync(ct);

            // paid customers (distinct)
            var paidCustomers = await (await _uow.Repository<Payment>().GetAllActives())
                .AsNoTracking()
                .Where(p => p.Invoice.OrganizationId == orgId && p.PaidAt >= fromUtc && p.PaidAt <= toUtc)
                .Select(p => p.Invoice.CustomerId)
                .Distinct()
                .CountAsync(ct);

            return new FunnelDto
            {
                Stages =
                [
                    new() { Name = "Signed Up", Value = signedUp },
            new() { Name = "Trial",     Value = trial },
            new() { Name = "Active",    Value = active },
            new() { Name = "Paid",      Value = paidCustomers },
        ]
            };
        }

        public async Task<IReadOnlyList<RecentEventDto>> GetRecentEventsAsync(Guid orgId, int take, CancellationToken ct)
        {
            // WebhookEventLog org bağı yok; org filtresi istersen tabloya OrganizationId ekleyelim.
            var q = (await _uow.Repository<WebhookEventLog>().GetAllActives())
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedDate)
                .Take(take <= 0 ? 20 : take);

            var list = await q.ToListAsync(ct);
            return list.Select(x => new RecentEventDto
            {
                Ts = DateTime.SpecifyKind(x.CreatedDate, DateTimeKind.Utc),
                Provider = x.Provider.ToString(),
                EventType = x.EventType,
                IsProcessed = x.IsProcessed,
                ProcessNote = x.ProcessNote
            }).ToList();
        }

        public async Task<IReadOnlyList<HealthDto>> GetHealthAsync(CancellationToken ct)
        {
            // Basit DB ping
            var started = DateTime.UtcNow;
            try
            {
                // en hafif sorgu: 1 adet id çek
                var anyInvoice = await (await _uow.Repository<Invoice>().GetAllActives())
                    .AsNoTracking()
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(ct);

                var dbLatency = (int)(DateTime.UtcNow - started).TotalMilliseconds;
                return new List<HealthDto>
            {
                new() { Name = "DB", Status = "Healthy", LatencyMs = dbLatency },
                new() { Name = "App", Status = "Healthy", LatencyMs = 1 }
            };
            }
            catch
            {
                var dbLatency = (int)(DateTime.UtcNow - started).TotalMilliseconds;
                return new List<HealthDto>
            {
                new() { Name = "DB", Status = "Unhealthy", LatencyMs = dbLatency },
                new() { Name = "App", Status = "Degraded", LatencyMs = 5 }
            };
            }
        }

        private static decimal NormalizeMonthly(decimal price, BillingPeriod period)
        {
            return period switch
            {
                BillingPeriod.Monthly => price,
                BillingPeriod.Quarterly => DecimalRound(price / 3m, 2),
                BillingPeriod.Yearly => DecimalRound(price / 12m, 2),
                BillingPeriod.Weekly => DecimalRound(price * 4.345m, 2), // 52.14/12
                BillingPeriod.Daily => DecimalRound(price * 30m, 2),
                _ => price
            };
        }

        private static decimal DecimalRound(decimal v, int digits) => Math.Round(v, digits, MidpointRounding.AwayFromZero);

        public async Task<IReadOnlyList<TopCustomerDto>> GetTopCustomersAsync(Guid orgId, DateTime fromUtc, DateTime toUtc, int take, CancellationToken ct)
        {
            var q = (await _uow.Repository<Payment>().GetAllActives()).AsNoTracking()
                .Where(p => p.Invoice.OrganizationId == orgId && p.PaidAt >= fromUtc && p.PaidAt <= toUtc);

            var list = await q.GroupBy(p => new { p.Invoice.CustomerId, p.Invoice.Customer!.Name })
                .Select(g => new TopCustomerDto { CustomerId = g.Key.CustomerId!.Value, Name = g.Key.Name, Total = g.Sum(x => x.Amount) })
                .OrderByDescending(x => x.Total)
                .Take(take <= 0 ? 5 : take)
                .ToListAsync(ct);

            return list;
        }

    }

}
