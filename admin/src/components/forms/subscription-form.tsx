"use client";
import { useForm } from "react-hook-form"; import { z } from "zod"; import { zodResolver } from "@hookform/resolvers/zod";
import { Input } from "@/components/ui/input"; import { Label } from "@/components/ui/label"; import { Button } from "@/components/ui/button";
import { api } from "@/lib/api"; import { toast } from "sonner";
const schema = z.object({
  organizationId: z.string().uuid(), appId: z.string().uuid(), planId: z.string().uuid(),
  customerId: z.string().uuid().optional(), startsAt: z.string().optional(), cancelAtPeriodEnd: z.boolean().optional()
});
type FormValues = z.infer<typeof schema>;

export function SubscriptionForm({ onCreated }:{ onCreated:()=>void }) {
  const { register, handleSubmit, reset, formState:{isSubmitting} } = useForm<FormValues>({ resolver: zodResolver(schema), defaultValues:{ cancelAtPeriodEnd:false } });
  const onSubmit = async (v:FormValues) => {
    const payload = { ...v, startsAt: v.startsAt ? new Date(v.startsAt).toISOString() : new Date().toISOString(), items: [] };
    await api.post("/subscriptions", payload); toast.success("Abonelik oluşturuldu"); reset(); onCreated();
  };
  return (
    <form className="grid gap-3" onSubmit={handleSubmit(onSubmit)}>
      <div className="grid grid-cols-2 gap-3">
        <div><Label>Org Id</Label><Input {...register("organizationId")} /></div>
        <div><Label>App Id</Label><Input {...register("appId")} /></div>
      </div>
      <div className="grid grid-cols-2 gap-3">
        <div><Label>Plan Id</Label><Input {...register("planId")} /></div>
        <div><Label>Müşteri Id (ops.)</Label><Input {...register("customerId")} /></div>
      </div>
      <div className="grid grid-cols-2 gap-3">
        <div><Label>Başlangıç</Label><Input type="datetime-local" {...register("startsAt")} /></div>
        <div className="flex items-end gap-2"><input id="cancel" type="checkbox" {...register("cancelAtPeriodEnd")} /><Label htmlFor="cancel">Dönem sonunda iptal</Label></div>
      </div>
      <Button disabled={isSubmitting}>{isSubmitting?"Kaydediliyor...":"Kaydet"}</Button>
    </form>
  );
}
