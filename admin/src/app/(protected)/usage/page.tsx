"use client";
import { useEffect, useState } from "react"; import { api } from "@/lib/api";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Paged, UsageRecordDto } from "@/types/dto"; import { Pagination } from "@/components/pagination"; import { PeriodUnitMap, fmt } from "@/lib/enums";

export default function UsagePage(){
  const [page,setPage]=useState(1), pageSize=50; const [subscriptionId,setSub]=useState(""), [featureId,setFeat]=useState("");
  const [data,setData]=useState<Paged<UsageRecordDto>|null>(null);
  const load = async ()=>{ const p:any={page,pageSize}; if(subscriptionId) p.subscriptionId=subscriptionId; if(featureId) p.featureId=featureId;
    const { data } = await api.get<Paged<UsageRecordDto>>(`/usage?${new URLSearchParams(p)}` as any); setData(data); };
  useEffect(()=>{ load().catch(console.error); },[page,subscriptionId,featureId]);
  return (
    <div className="grid gap-6">
      <Card>
        <CardHeader><CardTitle>Kullanım Kayıtları</CardTitle></CardHeader>
        <CardContent>
          <div className="flex gap-2 mb-3">
            <input placeholder="subscriptionId" className="border rounded px-2 py-1" value={subscriptionId} onChange={e=>setSub(e.target.value)} />
            <input placeholder="featureId" className="border rounded px-2 py-1" value={featureId} onChange={e=>setFeat(e.target.value)} />
            <button className="border rounded px-3" onClick={()=>{setPage(1); load();}}>Filtrele</button>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full text-sm"><thead><tr className="border-b text-left">
              <th className="py-2">Sub</th><th>Feature</th><th>Birim</th><th>Başlangıç</th><th>Bitiş</th><th>Değer</th></tr></thead>
              <tbody>{data?.items.map(x=>(
                <tr key={x.id} className="border-b">
                  <td className="py-2 font-mono text-xs">{x.subscriptionId}</td><td className="font-mono text-xs">{x.featureId}</td>
                  <td>{PeriodUnitMap[x.periodUnit] ?? x.periodUnit}</td><td>{fmt(x.periodStart)}</td><td>{fmt(x.periodEnd)}</td><td>{x.usedValue}</td>
                </tr>
              ))}{!data?.items.length && <tr><td colSpan={6} className="py-4 text-muted-foreground">Kayıt yok</td></tr>}</tbody>
            </table>
          </div>
          <div className="mt-3"><Pagination page={data?.page??page} pageSize={data?.pageSize??pageSize} total={data?.total??0} onPage={setPage} /></div>
        </CardContent>
      </Card>
    </div>
  );
}
