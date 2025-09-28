// ------------------------------
// 8) src/components/recent-events.tsx
// ------------------------------
"use client";
import { useEffect, useState } from "react";
import { api } from "@/lib/api";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";


export type RecentEvent = { ts: string; provider: string; eventType: string; isProcessed: boolean; processNote?: string | null };


export function RecentEvents({ orgId }: { orgId?: string | null }) {
const [items, setItems] = useState<RecentEvent[]>([]);


useEffect(() => {
api.get<RecentEvent[]>(`/dashboard/events/recent?take=20`).then(r => setItems(r.data)).catch(() => setItems([]));
}, [orgId]);


return (
<Card className="shadow-sm">
<CardHeader><CardTitle>Son Olaylar</CardTitle></CardHeader>
<CardContent>
<Table>
<TableHeader>
<TableRow>
<TableHead>Zaman</TableHead>
<TableHead>Sağlayıcı</TableHead>
<TableHead>Tür</TableHead>
<TableHead>Durum</TableHead>
</TableRow>
</TableHeader>
<TableBody>
{items.map((e, i) => (
<TableRow key={i}>
<TableCell>{new Date(e.ts).toLocaleString("tr-TR")}</TableCell>
<TableCell>{e.provider}</TableCell>
<TableCell>{e.eventType}</TableCell>
<TableCell>{e.isProcessed ? "İşlendi" : "Bekliyor"}</TableCell>
</TableRow>
))}
{!items.length && (
<TableRow><TableCell colSpan={4} className="text-muted-foreground">Kayıt yok</TableCell></TableRow>
)}
</TableBody>
</Table>
</CardContent>
</Card>
);
}