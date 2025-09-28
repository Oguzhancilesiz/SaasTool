"use client";
import { useEffect, useState } from "react"; import { api } from "@/lib/api";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { ListToolbar } from "@/components/list-toolbar"; import { Pagination } from "@/components/pagination";
import { ConfirmButton } from "@/components/confirm-button"; import { toCsv, downloadCsv } from "@/lib/csv";
import { OrganizationDto, Paged } from "@/types/dto"; import { OrgForm } from "@/components/forms/org-form"; import { toast } from "sonner";

export default function OrgsPage(){
  const [page,setPage]=useState(1), pageSize=20; const [q,setQ]=useState(""); const [data,setData]=useState<Paged<OrganizationDto>|null>(null);
  const load = async () => { const qs=new URLSearchParams({page:String(page),pageSize:String(pageSize),search:q}).toString(); const {data}=await api.get<Paged<OrganizationDto>>(`/organizations?${qs}`); setData(data); };
  useEffect(()=>{ load().catch(console.error); },[page,q]);
  const del = async (id:string)=>{ await api.delete(`/organizations/${id}`); toast.success("Silindi"); await load(); };
  const exportCsv=()=>{ const rows=(data?.items??[]).map(x=>({ Ad:x.name, Slug:x.slug??"" })); if(!rows.length) return toast.info("Kayıt yok"); downloadCsv("orgs.csv", toCsv(rows)); };
  return (
    <div className="grid gap-6">
      <Card><CardHeader><CardTitle>Yeni Organizasyon</CardTitle></CardHeader><CardContent><OrgForm onCreated={load}/></CardContent></Card>
      <Card>
        <CardHeader><CardTitle>Organizasyonlar</CardTitle><div className="mt-3"><ListToolbar onSearch={setQ} onRefresh={load} onExport={()=>exportCsv()} /></div></CardHeader>
        <CardContent>
          <div className="overflow-x-auto">
            <table className="w-full text-sm"><thead><tr className="border-b text-left"><th className="py-2">Ad</th><th>Slug</th><th></th></tr></thead>
              <tbody>{data?.items.map(x=>(
                <tr key={x.id} className="border-b"><td className="py-2">{x.name}</td><td>{x.slug??"-"}</td>
                  <td className="text-right"><ConfirmButton onConfirm={()=>del(x.id)}>Sil</ConfirmButton></td></tr>
              ))}{!data?.items.length && <tr><td colSpan={3} className="py-4 text-muted-foreground">Kayıt yok</td></tr>}</tbody>
            </table>
          </div>
          <div className="mt-3"><Pagination page={data?.page??page} pageSize={data?.pageSize??pageSize} total={data?.total??0} onPage={setPage}/></div>
        </CardContent>
      </Card>
    </div>
  );
}
