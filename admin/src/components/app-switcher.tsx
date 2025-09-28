// ------------------------------
// 7) src/components/app-switcher.tsx
// ------------------------------
"use client";
import { useEffect, useState } from "react";
import { useAtom } from "jotai";
import { orgAtom, setAppLocal } from "@/state/org";
import { api } from "@/lib/api";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";


type AppDto = { id: string; code: string; name: string };


type Paged<T> = { total: number; page: number; pageSize: number; items: T[] };


export function AppSwitcher() {
const [state, setState] = useAtom(orgAtom);
const [apps, setApps] = useState<AppDto[]>([]);


useEffect(() => {
api.get<Paged<AppDto>>("/apps?page=1&pageSize=100").then(r => setApps(r.data.items)).catch(() => setApps([]));
}, []);


const onChange = (val: string) => {
setState(s => ({ ...s, appId: val || null }));
setAppLocal(val || null);
};


return (
<Select value={state.appId ?? ""} onValueChange={onChange}>
<SelectTrigger className="w-[220px]">
<SelectValue placeholder="Uygulama seç (opsiyonel)" />
</SelectTrigger>
<SelectContent>
{apps.map(o => (
<SelectItem key={o.id} value={o.id}>{o.name} ({o.code})</SelectItem>
))}
</SelectContent>
</Select>
);
}