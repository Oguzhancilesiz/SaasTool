"use client";
import { useEffect, useState } from "react"; import { api } from "@/lib/api";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { ListToolbar } from "@/components/list-toolbar"; import { Pagination } from "@/components/pagination";
import { ConfirmButton } from "@/components/confirm-button"; import { toCsv, downloadCsv } from "@/lib/csv";
import { FeatureForm } from "@/components/forms/feature-form"; import { toast } from "sonner";
import { FeatureDto, Paged } from "@/types/dto";

export default function FeaturesPage(){
  const [page,setPage]=useState(1), pageSize=20; const [q,setQ]=useState(""); const [appId,setAppId]=useState<string>("");
  const [data,setData]=useState<Paged<FeatureDto>|null>(null);

  const load = async () => {
    const p:any = { page, pageSize }; if (q) p.search=q; if (appId) p.appId=appId;
    const qs = new URLSearchParams(Object.fromEntries(Object.entries(p).map(([k,v])=>[k,String(v)]))).toString();
    const { data } = await api.get<Paged<FeatureDto>>(`/features?${qs}`);
    setData(data);
  };
  useEffect(()=>{ load().catch(console.error); },[page,q,appId]);

  const del = async (id:string) => { await api.delete(`/features/${id}`); toast.success("Silindi"); await load(); };
  const exportCsv = () => {
    const rows = (data?.items ?? []).map(x=>({ App:x.appId, Kod:x.code, Ad:x.name }));
    if (!rows.length) return toast.info("Kayıt yok"); downloadCsv("features.csv", toCsv(rows));
  };

  return (
    <div className="grid gap-6">
      <Card><CardHeader><CardTitle>Yeni Özellik</CardTitle></CardHeader><CardContent><FeatureForm onCreated={load}/></CardContent></Card>
      <Card>
        <CardHeader>
          <CardTitle>Özellikler</CardTitle>
          <div className="mt-3"><ListToolbar onSearch={setQ} onRefresh={load} onExport={()=>exportCsv()} /></div>
        </CardHeader>
        <CardContent>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead><tr className="border-b text-left"><th className="py-2">App</th><th>Kod</th><th>Ad</th><th></th></tr></thead>
              <tbody>
                {data?.items.map(x=>(
                  <tr key={x.id} className="border-b">
                    <td className="py-2 font-mono text-xs">{x.appId}</td><td>{x.code}</td><td>{x.name}</td>
                    <td className="text-right"><ConfirmButton onConfirm={()=>del(x.id)}>Sil</ConfirmButton></td>
                  </tr>
                ))}
                {!data?.items.length && <tr><td colSpan={4} className="py-4 text-muted-foreground">Kayıt yok</td></tr>}
              </tbody>
            </table>
          </div>
          <div className="mt-3"><Pagination page={data?.page ?? page} pageSize={data?.pageSize ?? pageSize} total={data?.total ?? 0} onPage={setPage} /></div>
        </CardContent>
      </Card>
    </div>
  );
}
