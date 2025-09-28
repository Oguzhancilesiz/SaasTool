"use client";
import { useForm } from "react-hook-form"; import { z } from "zod"; import { zodResolver } from "@hookform/resolvers/zod";
import { Input } from "@/components/ui/input"; import { Label } from "@/components/ui/label"; import { Button } from "@/components/ui/button";
import { api } from "@/lib/api"; import { toast } from "sonner";

const schema = z.object({ invoiceId: z.string().uuid(), provider: z.number().int().min(0), amount: z.number().positive(), currency: z.number().int().min(0), paidAt: z.string().optional() });
type FormValues = z.infer<typeof schema>;

export function PaymentForm({ onCreated }:{ onCreated:()=>void }) {
  const { register, handleSubmit, reset, formState:{isSubmitting} } = useForm<FormValues>({ resolver: zodResolver(schema), defaultValues: { provider:0, currency:0 } });
  const onSubmit = async (v:FormValues)=>{ const payload={...v, paidAt: v.paidAt ? new Date(v.paidAt).toISOString() : new Date().toISOString()}; await api.post("/payments", payload); toast.success("Ödeme kaydedildi"); reset(); onCreated(); };
  return (
    <form className="grid gap-3" onSubmit={handleSubmit(onSubmit)}>
      <div className="grid grid-cols-2 gap-3">
        <div><Label>Invoice Id</Label><Input {...register("invoiceId")} /></div>
        <div><Label>Sağlayıcı</Label><Input type="number" {...register("provider",{valueAsNumber:true})} /></div>
      </div>
      <div className="grid grid-cols-3 gap-3">
        <div><Label>Tutar</Label><Input type="number" step="0.01" {...register("amount",{valueAsNumber:true})} /></div>
        <div><Label>Döviz</Label><Input type="number" {...register("currency",{valueAsNumber:true})} /></div>
        <div><Label>Tarih</Label><Input type="datetime-local" {...register("paidAt")} /></div>
      </div>
      <Button disabled={isSubmitting}>{isSubmitting?"Kaydediliyor...":"Kaydet"}</Button>
    </form>
  );
}
