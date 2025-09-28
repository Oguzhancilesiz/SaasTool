"use client";
import { useForm } from "react-hook-form"; import { z } from "zod"; import { zodResolver } from "@hookform/resolvers/zod";
import { Input } from "@/components/ui/input"; import { Label } from "@/components/ui/label"; import { Button } from "@/components/ui/button";
import { api } from "@/lib/api"; import { toast } from "sonner";

const schema = z.object({
  organizationId: z.string().uuid(), name: z.string().min(2), email: z.string().email(),
  taxNumber: z.string().optional(), billingAddress: z.string().optional(), country: z.string().optional(), city: z.string().optional()
});
type FormValues = z.infer<typeof schema>;

export function CustomerForm({ onCreated }:{ onCreated:()=>void }) {
  const { register, handleSubmit, reset, formState:{isSubmitting} } = useForm<FormValues>({ resolver: zodResolver(schema) });
  const onSubmit = async (v:FormValues) => { await api.post("/customers", v); toast.success("Müşteri eklendi"); reset(); onCreated(); };
  return (
    <form className="grid gap-3" onSubmit={handleSubmit(onSubmit)}>
      <div><Label>Organization Id</Label><Input {...register("organizationId")} placeholder="org guid" /></div>
      <div className="grid grid-cols-2 gap-3">
        <div><Label>Ad</Label><Input {...register("name")} /></div>
        <div><Label>Email</Label><Input {...register("email")} /></div>
      </div>
      <div className="grid grid-cols-2 gap-3">
        <div><Label>Vergi No</Label><Input {...register("taxNumber")} /></div>
        <div><Label>Şehir</Label><Input {...register("city")} /></div>
      </div>
      <div className="grid grid-cols-2 gap-3">
        <div><Label>Ülke</Label><Input {...register("country")} /></div>
        <div><Label>Adres</Label><Input {...register("billingAddress")} /></div>
      </div>
      <Button disabled={isSubmitting}>{isSubmitting?"Kaydediliyor...":"Kaydet"}</Button>
    </form>
  );
}
