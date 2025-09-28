"use client";
import { useEffect, useState } from "react"; import { api } from "@/lib/api";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { PaymentDto, Paged } from "@/types/dto"; import { Pagination } from "@/components/pagination"; import { PaymentForm } from "@/components/forms/payment-form";
import { money, fmt } from "@/lib/enums";

export default function PaymentsPage(){
  const [page,setPage]=useState(1), pageSize=20; const [data,setData]=useState<Paged<PaymentDto>|null>(null);
  // API'de list yok; ödeme hareketlerini faturadan görmek daha doğru. Demo için "son 50 ödeme"yi faturalar endpoint'inden genişletmek istersen ayrı API açarsın.
  const load = async ()=>{ /* opsiyonel: yoksa boş bırak. */ setData({ items:[], total:0, page, pageSize }); };
  useEffect(()=>{ load().catch(console.error); },[page]);
  return (
    <div className="grid gap-6">
      <Card><CardHeader><CardTitle>Yeni Ödeme</CardTitle></CardHeader><CardContent><PaymentForm onCreated={load}/></CardContent></Card>
      <Card><CardHeader><CardTitle>Ödemeler</CardTitle></CardHeader><CardContent><div className="text-sm text-muted-foreground">Ödemeleri faturalar üzerinden izleyin. İsterseniz Payments listesi için API ekleyelim.</div></CardContent></Card>
    </div>
  );
}
