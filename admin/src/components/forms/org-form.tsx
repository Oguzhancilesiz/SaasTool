"use client";
import { useForm } from "react-hook-form"; import { z } from "zod"; import { zodResolver } from "@hookform/resolvers/zod";
import { Input } from "@/components/ui/input"; import { Label } from "@/components/ui/label"; import { Button } from "@/components/ui/button";
import { api } from "@/lib/api"; import { toast } from "sonner";

const schema = z.object({ name: z.string().min(2), slug: z.string().optional() });
type FormValues = z.infer<typeof schema>;

export function OrgForm({ onCreated }:{ onCreated:()=>void }) {
  const { register, handleSubmit, reset, formState:{isSubmitting} } = useForm<FormValues>({ resolver: zodResolver(schema) });
  const onSubmit = async (v:FormValues) => { await api.post("/organizations", v); toast.success("Organizasyon eklendi"); reset(); onCreated(); };
  return (
    <form className="grid gap-3" onSubmit={handleSubmit(onSubmit)}>
      <div><Label>Ad</Label><Input {...register("name")} /></div>
      <div><Label>Slug</Label><Input {...register("slug")} /></div>
      <Button disabled={isSubmitting}>{isSubmitting?"Kaydediliyor...":"Kaydet"}</Button>
    </form>
  );
}
