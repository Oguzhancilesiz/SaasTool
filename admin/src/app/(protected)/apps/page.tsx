"use client";
import { useEffect, useState } from "react";
import { api } from "@/lib/api";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { ListToolbar } from "@/components/list-toolbar";
import { Pagination } from "@/components/pagination";
import { ConfirmButton } from "@/components/confirm-button";
import { toCsv, downloadCsv } from "@/lib/csv";
import { AppDto, Paged } from "@/types/dto";
import { AppForm } from "@/components/forms/app-form";
import { toast } from "sonner";

export default function AppsPage(){
  const [page,setPage]=useState(1); const pageSize=20;
  const [q,setQ]=useState(""); const [data,setData]=useState<Paged<AppDto>|null>(null);

  const load = async () => {
    const params = new URLSearchParams({ page:String(page), pageSize:String(pageSize), search:q });
    const { data } = await api.get<Paged<AppDto>>(`/apps?${params}`);
    setData(data);
  };
  useEffect(()=>{ load().catch(console.error); },[page,q]);

  const del = async (id:string) => { await api.delete(`/apps/${id}`); toast.success("Silindi"); await load(); };

  const exportCsv = () => {
    const rows = (data?.items ?? []).map(x => ({ Kod:x.code, Ad:x.name, Aktif:x.isEnabled ? "Evet":"Hayır" }));
    if (!rows.length) return toast.info("İhracat için kayıt yok");
    downloadCsv("apps.csv", toCsv(rows));
  };

  return (
    <div className="grid gap-6">
      <Card><CardHeader><CardTitle>Yeni Uygulama</CardTitle></CardHeader><CardContent><AppForm onCreated={load}/></CardContent></Card>
      <Card>
        <CardHeader>
          <CardTitle>Uygulamalar</CardTitle>
          <div className="mt-3"><ListToolbar onSearch={setQ} onRefresh={load} onExport={()=>exportCsv()} /></div>
        </CardHeader>
        <CardContent>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead><tr className="border-b text-left"><th className="py-2">Kod</th><th>Ad</th><th>Aktif</th><th></th></tr></thead>
              <tbody>
                {data?.items.map(x=>(
                  <tr key={x.id} className="border-b">
                    <td className="py-2">{x.code}</td><td>{x.name}</td><td>{x.isEnabled?"Evet":"Hayır"}</td>
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
