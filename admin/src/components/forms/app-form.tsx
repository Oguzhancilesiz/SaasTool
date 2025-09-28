"use client";
import { useForm } from "react-hook-form";
import { z } from "zod"; import { zodResolver } from "@hookform/resolvers/zod";
import { Input } from "@/components/ui/input"; import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button"; import { api } from "@/lib/api";
import { toast } from "sonner"; import { AppCreate } from "@/types/dto";

const schema = z.object({ code: z.string().min(2), name: z.string().min(2), isEnabled: z.boolean().optional().default(true) });
type FormValues = z.infer<typeof schema>;

export function AppForm({ onCreated }: { onCreated: ()=>void }) {
  const { register, handleSubmit, reset, formState:{isSubmitting} } = useForm<FormValues>({
    resolver: zodResolver(schema), defaultValues: { isEnabled: true }
  });

  const onSubmit = async (v: FormValues) => {
    await api.post("/apps", v as AppCreate);
    toast.success("Uygulama oluşturuldu"); reset(); onCreated();
  };

  return (
    <form className="grid gap-3" onSubmit={handleSubmit(onSubmit)}>
      <div><Label>Kod</Label><Input {...register("code")} /></div>
      <div><Label>Ad</Label><Input {...register("name")} /></div>
      <div className="flex items-center gap-2">
        <input id="isen" type="checkbox" {...register("isEnabled")} /><Label htmlFor="isen">Aktif</Label>
      </div>
      <Button disabled={isSubmitting}>{isSubmitting?"Kaydediliyor...":"Kaydet"}</Button>
    </form>
  );
}
