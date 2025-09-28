"use client";
import { useEffect, useState } from "react"; import { api } from "@/lib/api";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { ListToolbar } from "@/components/list-toolbar"; import { Pagination } from "@/components/pagination";
import { SubscriptionDto, Paged } from "@/types/dto"; import { SubscriptionForm } from "@/components/forms/subscription-form";
import { fmt } from "@/lib/enums";

export default function SubscriptionsPage(){
  const [page,setPage]=useState(1), pageSize=20; const [orgId,setOrgId]=useState("");
  const [data,setData]=useState<Paged<SubscriptionDto>|null>(null);
  const load = async()=>{ const p:any={page,pageSize}; if(orgId) p.organizationId=orgId; const { data } = await api.get<Paged<SubscriptionDto>>(`/subscriptions?${new URLSearchParams(p)}` as any); setData(data); };
  useEffect(()=>{ load().catch(console.error); },[page,orgId]);
  return (
    <div className="grid gap-6">
      <Card><CardHeader><CardTitle>Yeni Abonelik</CardTitle></CardHeader><CardContent><SubscriptionForm onCreated={load}/></CardContent></Card>
      <Card>
        <CardHeader><CardTitle>Abonelikler</CardTitle><div className="text-sm text-muted-foreground mt-1">Liste yalnızca temel kolonları gösterir.</div></CardHeader>
        <CardContent>
          <div className="overflow-x-auto">
            <table className="w-full text-sm"><thead><tr className="border-b text-left"><th className="py-2">Org</th><th>App</th><th>Plan</th><th>Müşteri</th><th>Başlangıç</th><th>Bitiş</th></tr></thead>
            <tbody>{data?.items.map(x=>(
              <tr key={x.id} className="border-b"><td className="py-2 font-mono text-xs">{x.organizationId}</td>
              <td className="font-mono text-xs">{x.appId}</td><td className="font-mono text-xs">{x.planId}</td>
              <td className="font-mono text-xs">{x.customerId ?? "-"}</td><td>{fmt(x.startsAt)}</td><td>{fmt(x.endsAt)}</td></tr>
            ))}{!data?.items.length && <tr><td colSpan={6} className="py-4 text-muted-foreground">Kayıt yok</td></tr>}</tbody></table>
          </div>
          <div className="mt-3"><Pagination page={data?.page??page} pageSize={data?.pageSize??pageSize} total={data?.total??0} onPage={setPage} /></div>
        </CardContent>
      </Card>
    </div>
  );
}
