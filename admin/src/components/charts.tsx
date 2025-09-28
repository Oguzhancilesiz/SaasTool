"use client";

import {
  ResponsiveContainer,
  AreaChart,
  Area,
  XAxis,
  YAxis,
  Tooltip,
  CartesianGrid,
  LineChart,
  Line,
  PieChart,
  Pie,
  Cell,
  Legend,
} from "recharts";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

// Basit başlık/kap
export function ChartCard({
  title,
  children,
  right,
}: {
  title: string;
  children: React.ReactNode;
  right?: React.ReactNode;
}) {
  return (
    <Card className="shadow-sm">
      <CardHeader className="flex-row items-center justify-between gap-2">
        <CardTitle className="text-base">{title}</CardTitle>
        {right}
      </CardHeader>
      <CardContent className="h-[280px]">{children}</CardContent>
    </Card>
  );
}

// Tooltipler
export const CurrencyTooltip = ({ label, payload }: any) => (
  <div className="rounded-md border bg-popover p-2 text-xs text-popover-foreground shadow-sm">
    <div className="mb-1 font-medium opacity-80">{label}</div>
    {payload?.map((p: any, i: number) => (
      <div key={i} className="flex items-center gap-2">
        <span
          className="inline-block h-2 w-2 rounded-sm"
          style={{ background: p.color }}
        />
        <span className="opacity-70">{p.name}:</span>
        <span className="font-medium">
          {new Intl.NumberFormat("tr-TR", {
            style: "currency",
            currency: "TRY",
          }).format(p.value ?? 0)}
        </span>
      </div>
    ))}
  </div>
);

export const NumberTooltip = ({ label, payload }: any) => (
  <div className="rounded-md border bg-popover p-2 text-xs text-popover-foreground shadow-sm">
    <div className="mb-1 font-medium opacity-80">{label}</div>
    {payload?.map((p: any, i: number) => (
      <div key={i} className="flex items-center gap-2">
        <span
          className="inline-block h-2 w-2 rounded-sm"
          style={{ background: p.color }}
        />
        <span className="opacity-70">{p.name}:</span>
        <span className="font-medium">
          {new Intl.NumberFormat("tr-TR").format(p.value ?? 0)}
        </span>
      </div>
    ))}
  </div>
);

// 1) Gelir alan grafiği
export function RevenueArea({ data }: { data: { t: string; v: number }[] }) {
  return (
    <ResponsiveContainer width="100%" height="100%">
      <AreaChart data={data} margin={{ left: 8, right: 8, top: 8, bottom: 0 }}>
        <defs>
          <linearGradient id="rev" x1="0" y1="0" x2="0" y2="1">
            <stop offset="5%" stopColor="var(--chart-1)" stopOpacity={0.35} />
            <stop offset="95%" stopColor="var(--chart-1)" stopOpacity={0} />
          </linearGradient>
        </defs>
        <CartesianGrid vertical={false} strokeOpacity={0.15} />
        <XAxis dataKey="t" tick={{ fontSize: 12 }} tickMargin={8} />
        <YAxis tick={{ fontSize: 12 }} width={60} />
        <Tooltip content={<CurrencyTooltip />} />
        <Area
          type="monotone"
          dataKey="v"
          name="Gelir"
          stroke="var(--chart-1)"
          fill="url(#rev)"
          strokeWidth={2}
        />
      </AreaChart>
    </ResponsiveContainer>
  );
}

// 2) Abonelik çizgi grafiği
export function SubscriptionsLine({
  data,
}: {
  data: { t: string; v: number }[];
}) {
  return (
    <ResponsiveContainer width="100%" height="100%">
      <LineChart data={data} margin={{ left: 8, right: 8, top: 8, bottom: 0 }}>
        <CartesianGrid vertical={false} strokeOpacity={0.15} />
        <XAxis dataKey="t" tick={{ fontSize: 12 }} tickMargin={8} />
        <YAxis tick={{ fontSize: 12 }} width={40} />
        <Tooltip content={<NumberTooltip />} />
        <Line
          type="monotone"
          dataKey="v"
          name="Yeni Abonelik"
          stroke="var(--chart-2)"
          strokeWidth={2}
          dot={false}
        />
      </LineChart>
    </ResponsiveContainer>
  );
}

// 3) Plan dağılımı pie
export function PlanPie({
  data,
}: {
  data: { key: string; value: number }[];
}) {
  const COLORS = [
    "var(--chart-3)",
    "var(--chart-4)",
    "var(--chart-5)",
    "var(--chart-2)",
    "var(--chart-1)",
  ];
  return (
    <ResponsiveContainer width="100%" height="100%">
      <PieChart>
        <Tooltip content={<NumberTooltip />} />
        <Legend verticalAlign="bottom" height={28} />
        <Pie
          data={data}
          dataKey="value"
          nameKey="key"
          innerRadius={60}
          outerRadius={90}
          paddingAngle={4}
        >
          {data.map((_, i) => (
            <Cell key={i} fill={COLORS[i % COLORS.length]} />
          ))}
        </Pie>
      </PieChart>
    </ResponsiveContainer>
  );
}
