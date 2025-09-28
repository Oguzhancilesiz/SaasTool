"use client";
import { useEffect, useState } from "react"; import { api } from "@/lib/api";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Pagination } from "@/components/pagination"; import { InvoiceDto, Paged } from "@/types/dto";
import { InvoiceForm } from "@/components/forms/invoice-form"; import { CurrencyMap, fmt, money } from "@/lib/enums";

export default function InvoicesPage(){
  const [page,setPage]=useState(1), pageSize=20; const [orgId,setOrgId]=useState(""); const [data,setData]=useState<Paged<InvoiceDto>|null>(null);
  const load = async()=>{ const p:any={page,pageSize}; if(orgId) p.organizationId=orgId; const { data } = await api.get<Paged<InvoiceDto>>(`/invoices?${new URLSearchParams(p)}` as any); setData(data); };
  useEffect(()=>{ load().catch(console.error); },[page,orgId]);

  return (
    <div className="grid gap-6">
      <Card><CardHeader><CardTitle>Yeni Fatura</CardTitle></CardHeader><CardContent><InvoiceForm onCreated={load}/></CardContent></Card>
      <Card>
        <CardHeader><CardTitle>Faturalar</CardTitle></CardHeader>
        <CardContent>
          <div className="overflow-x-auto">
            <table className="w-full text-sm"><thead><tr className="border-b text-left"><th className="py-2">No</th><th>Org</th><th>Müşteri</th><th>Tutar</th><th>Vade</th><th>Ödendi</th></tr></thead>
            <tbody>{data?.items.map(x=>(
              <tr key={x.id} className="border-b">
                <td className="py-2">{x.invoiceNumber}</td><td className="font-mono text-xs">{x.organizationId}</td>
                <td className="font-mono text-xs">{x.customerId ?? "-"}</td><td>{money(x.grandTotal, x.currency)}</td>
                <td>{fmt(x.dueDate)}</td><td>{fmt(x.paidAt)}</td>
              </tr>
            ))}{!data?.items.length && <tr><td colSpan={6} className="py-4 text-muted-foreground">Kayıt yok</td></tr>}</tbody></table>
          </div>
          <div className="mt-3"><Pagination page={data?.page??page} pageSize={data?.pageSize??pageSize} total={data?.total??0} onPage={setPage} /></div>
        </CardContent>
      </Card>
    </div>
  );
}
