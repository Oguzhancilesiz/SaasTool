// ------------------------------
// 12) src/app/(protected)/dashboard/page.tsx
// ------------------------------
"use client";
import { useEffect, useMemo, useState } from "react";
import { useAtomValue } from "jotai";
import { orgAtom } from "@/state/org";
import { api } from "@/lib/api";
import { fmtMoneyTRY, fmtNumber } from "@/lib/format";
import { KpiCard } from "@/components/kpi-card";
import { ChartCard, PlanPie, RevenueArea, SubscriptionsLine } from "@/components/charts";
import { DateRangePicker, Range, presetRange } from "@/components/date-range";
import { RecentEvents } from "@/components/recent-events";
import { HealthCards } from "@/components/health-cards";
import { Card, CardContent } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { TrendingUp, Users, Wallet, LineChart } from "lucide-react";
import { Kpi } from "@/components/kpi";
import { TopCustomers } from "@/components/top-customers";
// --- DTOs matching your backend ---

type KpiDto = {
  revenueToday: number;
  revenueMTD: number;
  newCustomers7d: number;
  activeSubscriptions: number;
  mrr: number;
  churnRate: number; // 0..1
};

// /dashboard/timeseries/revenue & /subscriptions
// We'll map to { t: string, v: number } where t is formatted date

export default function DashboardPage() {
  const { orgId, appId } = useAtomValue(orgAtom);

  // redirect hint: require org selection
  const [range, setRange] = useState<Range>(() => presetRange("30d"));
  const [kpi, setKpi] = useState<KpiDto | null>(null);
  const [revenue, setRevenue] = useState<{ t: string; v: number }[]>([]);
  const [subs, setSubs] = useState<{ t: string; v: number }[]>([]);
  const [plans, setPlans] = useState<{ key: string; value: number }[]>([]);
  const [loading, setLoading] = useState(false);

  const params = useMemo(() => {
    const qs = new URLSearchParams();
    if (orgId) qs.set("orgId", orgId);
    if (appId) qs.set("appId", appId);
    qs.set("fromUtc", new Date(range.from).toISOString());
    qs.set("toUtc", new Date(range.to).toISOString());
    return qs.toString();
  }, [orgId, appId, range]);

  useEffect(() => {
    if (!orgId) return; // require org selection
    setLoading(true);
    const fetchAll = async () => {
      const [kpis, rev, srs, brk] = await Promise.all([
        api.get<KpiDto>(`/dashboard/kpis?orgId=${orgId}${appId ? `&appId=${appId}` : ""}`),
        api.get<{ t: string; v: number }[]>(`/dashboard/timeseries/revenue?${params}`),
        api.get<{ t: string; v: number }[]>(`/dashboard/timeseries/subscriptions?${params}`),
        api.get<{ key: string; value: number }[]>(`/dashboard/breakdown/plans?${params}`),
      ]);
      setKpi(kpis.data);
      setRevenue(rev.data.map(d => ({ t: d.t.slice(0, 10), v: d.v })));
      setSubs(srs.data.map(d => ({ t: d.t.slice(0, 10), v: d.v })));
      setPlans(brk.data);
    };
    fetchAll().catch(console.error).finally(() => setLoading(false));
  }, [orgId, appId, params]);

  // basic trend calc vs previous 7 days for RevenueToday & NewCustomers if we have series
  const revenueTrend = useMemo(() => {
    if (!revenue.length) return null;
    const last = revenue.at(-1)?.v ?? 0;
    const prev = revenue.at(-2)?.v ?? last;
    const deltaPct = prev ? ((last - prev) / prev) * 100 : 0;
    return { deltaPct };
  }, [revenue]);

  const subsTrend = useMemo(() => {
    if (!subs.length) return null;
    const last = subs.at(-1)?.v ?? 0;
    const prev = subs.at(-2)?.v ?? last;
    const deltaPct = prev ? ((last - prev) / prev) * 100 : 0;
    return { deltaPct };
  }, [subs]);

  return (
    <div className="grid gap-6">
      {!orgId && (
        <Card className="border-amber-500/40">
          <CardContent className="p-4 text-sm">Lütfen üst bardan bir organizasyon seçin. Onsuz veri yok, sihirbaz değilim.</CardContent>
        </Card>
      )}

      {/* Controls */}
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div className="text-lg font-semibold">Genel Bakış</div>
        <DateRangePicker value={range} onChange={setRange} />
      </div>

      {/* KPI Row */}
      <div className="grid gap-4 md:grid-cols-5">
        <KpiCard title="Bugün Gelir" value={kpi ? fmtMoneyTRY(kpi.revenueToday) : <Skeleton className="h-8 w-24" />} trend={revenueTrend} icon={<Wallet size={16} />} />
        <KpiCard title="MTD Gelir" value={kpi ? fmtMoneyTRY(kpi.revenueMTD) : <Skeleton className="h-8 w-24" />} icon={<TrendingUp size={16} />} />
        <KpiCard title="MRR" value={kpi ? fmtMoneyTRY(kpi.mrr) : <Skeleton className="h-8 w-24" />} subtitle="Aylık yineleyen gelir" icon={<LineChart size={16} />} />
        <KpiCard title="Aktif Abonelik" value={kpi ? fmtNumber(kpi.activeSubscriptions) : <Skeleton className="h-8 w-12" />} trend={subsTrend} icon={<Users size={16} />} />
        <KpiCard title="Churn" value={kpi ? `${(kpi.churnRate * 100).toFixed(2)}%` : <Skeleton className="h-8 w-16" />} subtitle="Bu ay" />
      </div>

      {/* Charts Row */}
      <div className="grid gap-6 md:grid-cols-3">
        <ChartCard title="Gelir (TRY)">
          <RevenueArea data={revenue} />
        </ChartCard>
        <ChartCard title="Yeni Abonelikler">
          <SubscriptionsLine data={subs} />
        </ChartCard>
        <ChartCard title="Plan Kırılımı">
          <PlanPie data={plans} />
        </ChartCard>
      </div>

      {/* Ops Row */}
      <div className="grid gap-6 md:grid-cols-3">
        <div className="md:col-span-2"><RecentEvents orgId={orgId ?? undefined} /></div>
        <div><HealthCards /></div>
      </div>
    </div>
  );
}