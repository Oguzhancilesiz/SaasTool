"use client";
import { useForm, useFieldArray } from "react-hook-form"; import { z } from "zod"; import { zodResolver } from "@hookform/resolvers/zod";
import { Input } from "@/components/ui/input"; import { Label } from "@/components/ui/label"; import { Button } from "@/components/ui/button";
import { api } from "@/lib/api"; import { toast } from "sonner";

const lineSchema = z.object({ description:z.string().min(1), quantity:z.number().positive(), unitPrice:z.number().nonnegative() });
const schema = z.object({
  organizationId: z.string().uuid(),
  customerId: z.string().uuid().optional(),
  currency: z.number().int().min(0),
  dueDate: z.string().optional(),
  provider: z.number().int().min(0).default(0),
  lines: z.array(lineSchema).min(1)
});
type FormValues = z.infer<typeof schema>;

export function InvoiceForm({ onCreated }:{ onCreated:()=>void }) {
  const { register, handleSubmit, reset, control, formState:{isSubmitting} } = useForm<FormValues>({
    resolver: zodResolver(schema), defaultValues: { currency:0, provider:0, lines:[{description:"Satır", quantity:1, unitPrice:0}] }
  });
  const { fields, append, remove } = useFieldArray({ control, name:"lines" });

  const onSubmit = async (v:FormValues) => {
    const payload = { ...v, dueDate: v.dueDate ? new Date(v.dueDate).toISOString() : null };
    await api.post("/invoices", payload); toast.success("Fatura oluşturuldu"); reset(); onCreated();
  };

  return (
    <form className="grid gap-3" onSubmit={handleSubmit(onSubmit)}>
      <div className="grid grid-cols-2 gap-3">
        <div><Label>Org Id</Label><Input {...register("organizationId")} /></div>
        <div><Label>Müşteri Id (ops)</Label><Input {...register("customerId")} /></div>
      </div>
      <div className="grid grid-cols-3 gap-3">
        <div><Label>Döviz (0=TRY,1=USD,2=EUR)</Label><Input type="number" {...register("currency", { valueAsNumber:true })} /></div>
        <div><Label>Vade</Label><Input type="datetime-local" {...register("dueDate")} /></div>
        <div><Label>Sağlayıcı</Label><Input type="number" {...register("provider", { valueAsNumber:true })} /></div>
      </div>

      <div className="border rounded p-3">
        <div className="text-sm font-medium mb-2">Satırlar</div>
        {fields.map((f,idx)=>(
          <div key={f.id} className="grid grid-cols-5 gap-2 items-end mb-2">
            <div className="col-span-3"><Label>Açıklama</Label><Input {...register(`lines.${idx}.description` as const)} /></div>
            <div><Label>Adet</Label><Input type="number" {...register(`lines.${idx}.quantity` as const, { valueAsNumber:true })} /></div>
            <div><Label>Birim Fiyat</Label><Input type="number" step="0.01" {...register(`lines.${idx}.unitPrice` as const, { valueAsNumber:true })} /></div>
            <button type="button" className="text-xs text-red-600" onClick={()=>remove(idx)}>Sil</button>
          </div>
        ))}
        <Button type="button" variant="outline" onClick={()=>append({description:"Satır", quantity:1, unitPrice:0})}>Satır ekle</Button>
      </div>

      <Button disabled={isSubmitting}>{isSubmitting?"Kaydediliyor...":"Kaydet"}</Button>
    </form>
  );
}
