// src/components/kpi.tsx
"use client";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export function Kpi({
  title,
  value,
  pct,              // trend yüzde: +/-
  subtitle,
}: {
  title: string;
  value: string;
  pct?: number | null;
  subtitle?: string;
}) {
  const up = (pct ?? 0) >= 0;
  return (
    <Card className="shadow-sm">
      <CardHeader className="pb-2 flex-row items-center justify-between">
        <CardTitle className="text-sm text-muted-foreground">{title}</CardTitle>
        {pct != null && (
          <Badge variant={up ? "default" : "destructive"}>
            {up ? "↑" : "↓"} {Math.abs(pct!).toFixed(1)}%
          </Badge>
        )}
      </CardHeader>
      <CardContent>
        <div className="text-[28px] leading-[32px] font-semibold">{value}</div>
        {!!subtitle && <div className="text-xs mt-1 text-muted-foreground">{subtitle}</div>}
      </CardContent>
    </Card>
  );
}
