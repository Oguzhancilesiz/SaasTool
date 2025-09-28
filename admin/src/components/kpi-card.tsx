// ------------------------------
// 3) src/components/kpi-card.tsx
// ------------------------------
"use client";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { cn } from "@/lib/utils";


export function KpiCard({
title,
value,
subtitle,
trend,
loading,
icon,
}: {
title: string;
value: React.ReactNode;
subtitle?: React.ReactNode;
trend?: { deltaPct: number; label?: string } | null;
loading?: boolean;
icon?: React.ReactNode;
}) {
const up = (trend?.deltaPct ?? 0) > 0;
const down = (trend?.deltaPct ?? 0) < 0;
return (
<Card className={cn("shadow-sm overflow-hidden", loading && "opacity-70")}>
<CardHeader className="pb-1 flex flex-row items-center justify-between gap-2">
<CardTitle className="text-sm text-muted-foreground flex items-center gap-2">
{icon}
{title}
</CardTitle>
{trend && (
<span
className={cn(
"rounded-full px-2 py-0.5 text-xs",
up && "bg-emerald-500/10 text-emerald-600 dark:text-emerald-400",
down && "bg-red-500/10 text-red-600 dark:text-red-400",
!up && !down && "bg-muted text-muted-foreground"
)}
>
{trend.deltaPct > 0 ? "▲" : trend.deltaPct < 0 ? "▼" : "—"} {Math.abs(trend.deltaPct).toFixed(1)}%
</span>
)}
</CardHeader>
<CardContent className="pt-0">
<div className="text-3xl font-semibold tracking-tight">{value}</div>
{subtitle && <div className="text-xs text-muted-foreground mt-1">{subtitle}</div>}
</CardContent>
</Card>
);
}