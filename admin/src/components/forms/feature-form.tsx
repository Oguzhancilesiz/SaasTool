"use client";
import { useForm } from "react-hook-form"; import { z } from "zod"; import { zodResolver } from "@hookform/resolvers/zod";
import { Input } from "@/components/ui/input"; import { Label } from "@/components/ui/label"; import { Textarea } from "@/components/ui/textarea";
import { Button } from "@/components/ui/button"; import { api } from "@/lib/api"; import { toast } from "sonner";

const schema = z.object({ appId: z.string().uuid(), code: z.string().min(2), name: z.string().min(2), description: z.string().optional() });
type FormValues = z.infer<typeof schema>;

export function FeatureForm({ onCreated }:{ onCreated:()=>void }) {
  const { register, handleSubmit, reset, formState:{isSubmitting} } = useForm<FormValues>({ resolver: zodResolver(schema) });
  const onSubmit = async (v:FormValues) => { await api.post("/features", v); toast.success("Özellik eklendi"); reset(); onCreated(); };
  return (
    <form className="grid gap-3" onSubmit={handleSubmit(onSubmit)}>
      <div><Label>App Id</Label><Input {...register("appId")} placeholder="app guid" /></div>
      <div className="grid grid-cols-2 gap-3">
        <div><Label>Kod</Label><Input {...register("code")} /></div>
        <div><Label>Ad</Label><Input {...register("name")} /></div>
      </div>
      <div><Label>Açıklama</Label><Textarea rows={3} {...register("description")} /></div>
      <Button disabled={isSubmitting}>{isSubmitting?"Kaydediliyor...":"Kaydet"}</Button>
    </form>
  );
}
