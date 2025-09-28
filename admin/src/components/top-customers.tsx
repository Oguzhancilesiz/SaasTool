// ------------------------------
// 10) src/components/top-customers.tsx (optional)
// ------------------------------
"use client";
import { useEffect, useState } from "react";
import { api } from "@/lib/api";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";


// Placeholder demo: requires a backend endpoint later. For now hidden usage.
export function TopCustomers() {
const [rows, setRows] = useState<{ name: string; total: number }[]>([]);
useEffect(() => { setRows([]); }, []);
if (!rows.length) return null;
return (
<Card className="shadow-sm">
<CardHeader><CardTitle>En Çok Gelir Getiren Müşteriler</CardTitle></CardHeader>
<CardContent>
<Table>
<TableHeader>
<TableRow>
<TableHead>Müşteri</TableHead>
<TableHead className="text-right">Toplam</TableHead>
</TableRow>
</TableHeader>
<TableBody>
{rows.map((r, i) => (
<TableRow key={i}>
<TableCell className="font-medium">{r.name}</TableCell>
<TableCell className="text-right">{new Intl.NumberFormat("tr-TR", { style: "currency", currency: "TRY" }).format(r.total)}</TableCell>
</TableRow>
))}
</TableBody>
</Table>
</CardContent>
</Card>
);
}