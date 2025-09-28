"use client";
import { useEffect, useState } from "react"; import { api } from "@/lib/api";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { ListToolbar } from "@/components/list-toolbar"; import { Pagination } from "@/components/pagination";
import { ConfirmButton } from "@/components/confirm-button"; import { toCsv, downloadCsv } from "@/lib/csv";
import { CustomerDto, Paged } from "@/types/dto"; import { CustomerForm } from "@/components/forms/customer-form"; import { toast } from "sonner";

export default function CustomersPage(){
  const [page,setPage]=useState(1), pageSize=20; const [q,setQ]=useState(""); const [orgId,setOrgId]=useState("");
  const [data,setData]=useState<Paged<CustomerDto>|null>(null);

  const load = async ()=>{ const p:any={page,pageSize}; if(orgId) p.organizationId=orgId; if(q) p.search=q;
    const { data } = await api.get<Paged<CustomerDto>>(`/customers?${new URLSearchParams(p)}` as any);
    setData(data);
  };
  useEffect(()=>{ load().catch(console.error); },[page,q,orgId]);

  const del = async (id:string)=>{ await api.delete(`/customers/${id}`); toast.success("Silindi"); await load(); };
  const exportCsv=()=>{ const rows=(data?.items??[]).map(x=>({ Ad:x.name, Email:x.email, Org:x.organizationId })); if(!rows.length) return toast.info("Kayıt yok"); downloadCsv("customers.csv", toCsv(rows)); };

  return (
    <div className="grid gap-6">
      <Card><CardHeader><CardTitle>Yeni Müşteri</CardTitle></CardHeader><CardContent><CustomerForm onCreated={load}/></CardContent></Card>
      <Card>
        <CardHeader><CardTitle>Müşteriler</CardTitle><div className="mt-3"><ListToolbar onSearch={setQ} onRefresh={load} onExport={()=>exportCsv()} /></div></CardHeader>
        <CardContent>
          <div className="overflow-x-auto">
            <table className="w-full text-sm"><thead><tr className="border-b text-left"><th className="py-2">Ad</th><th>Email</th><th>Org</th><th></th></tr></thead>
            <tbody>{data?.items.map(x=>(
              <tr key={x.id} className="border-b"><td className="py-2">{x.name}</td><td>{x.email}</td><td className="font-mono text-xs">{x.organizationId}</td>
                <td className="text-right"><ConfirmButton onConfirm={()=>del(x.id)}>Sil</ConfirmButton></td></tr>
            ))}{!data?.items.length && <tr><td colSpan={4} className="py-4 text-muted-foreground">Kayıt yok</td></tr>}</tbody></table>
          </div>
          <div className="mt-3"><Pagination page={data?.page??page} pageSize={data?.pageSize??pageSize} total={data?.total??0} onPage={setPage} /></div>
        </CardContent>
      </Card>
    </div>
  );
}
