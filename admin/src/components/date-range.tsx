// ------------------------------
// 5) src/components/date-range.tsx
// ------------------------------
"use client";
import { useMemo, useState } from "react";
import { addMonths, startOfQuarter, startOfYear, subDays } from "date-fns";
import { Button } from "@/components/ui/button";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { Calendar } from "@/components/ui/calendar";
import { cn } from "@/lib/utils";


export type Range = { from: Date; to: Date };


export function presetRange(name: "7d" | "30d" | "QTD" | "YTD"): Range {
const now = new Date();
if (name === "7d") return { from: subDays(now, 6), to: now };
if (name === "30d") return { from: subDays(now, 29), to: now };
if (name === "QTD") return { from: startOfQuarter(now), to: now };
return { from: startOfYear(now), to: now };
}


export function DateRangePicker({ value, onChange }: { value: Range; onChange: (r: Range) => void }) {
const [open, setOpen] = useState(false);
const formatted = useMemo(() => {
const fmt = (d: Date) => d.toISOString().slice(0, 10);
return `${fmt(value.from)} → ${fmt(value.to)}`;
}, [value]);


return (
<div className="flex items-center gap-2">
{(["7d","30d","QTD","YTD"] as const).map(p => (
<Button key={p} size="sm" variant="secondary" onClick={() => onChange(presetRange(p))}>{p}</Button>
))}
<Popover open={open} onOpenChange={setOpen}>
<PopoverTrigger asChild>
<Button size="sm" variant="outline" className="whitespace-nowrap">{formatted}</Button>
</PopoverTrigger>
<PopoverContent align="start" className="w-auto">
<div className="grid grid-cols-2 gap-3">
<Calendar mode="single" selected={value.from} onSelect={(d) => d && onChange({ ...value, from: d })} />
<Calendar mode="single" selected={value.to} onSelect={(d) => d && onChange({ ...value, to: d })} />
</div>
</PopoverContent>
</Popover>
</div>
);
}