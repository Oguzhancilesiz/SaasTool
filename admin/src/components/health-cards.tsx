// ------------------------------
// 9) src/components/health-cards.tsx
// ------------------------------
"use client";
import { useEffect, useState } from "react";
import { api } from "@/lib/api";
import { Card, CardContent } from "@/components/ui/card";
import { cn } from "@/lib/utils";


export type HealthDto = { name: string; status: string; latencyMs: number };


export function HealthCards() {
const [list, setList] = useState<HealthDto[]>([]);
useEffect(() => { api.get<HealthDto[]>("/dashboard/health").then(r => setList(r.data)).catch(() => setList([])); }, []);
return (
<div className="grid gap-2 sm:grid-cols-2">
{list.map(h => (
<Card key={h.name} className={cn("border", h.status === "Healthy" ? "border-emerald-500/40" : h.status === "Degraded" ? "border-amber-500/40" : "border-red-500/40")}>
<CardContent className="p-3 text-sm flex items-center justify-between">
<div className="font-medium">{h.name}</div>
<div className={cn("rounded-full px-2 py-0.5", h.status === "Healthy" && "bg-emerald-500/10 text-emerald-500", h.status === "Degraded" && "bg-amber-500/10 text-amber-500", h.status === "Unhealthy" && "bg-red-500/10 text-red-500")}>{h.status}</div>
<div className="text-muted-foreground">{h.latencyMs} ms</div>
</CardContent>
</Card>
))}
{!list.length && <div className="text-sm text-muted-foreground">Sağlık verisi yok</div>}
</div>
);
}