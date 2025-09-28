// ------------------------------
// 6) src/components/org-switcher.tsx
// ------------------------------
"use client";
import { useEffect, useState } from "react";
import { useAtom } from "jotai";
import { orgAtom, setOrgLocal } from "@/state/org";
import { api } from "@/lib/api";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";


type OrgDto = { id: string; name: string };


type Paged<T> = { total: number; page: number; pageSize: number; items: T[] };


export function OrgSwitcher() {
const [state, setState] = useAtom(orgAtom);
const [orgs, setOrgs] = useState<OrgDto[]>([]);


useEffect(() => {
api.get<Paged<OrgDto>>("/organizations?page=1&pageSize=100").then(r => setOrgs(r.data.items)).catch(() => setOrgs([]));
}, []);


const onChange = (val: string) => {
setState(s => ({ ...s, orgId: val || null }));
setOrgLocal(val || null);
};


return (
<Select value={state.orgId ?? ""} onValueChange={onChange}>
<SelectTrigger className="w-[220px]">
<SelectValue placeholder="Organizasyon seç" />
</SelectTrigger>
<SelectContent>
{orgs.map(o => (
<SelectItem key={o.id} value={o.id}>{o.name}</SelectItem>
))}
</SelectContent>
</Select>
);
}