// src/components/forms/feature-form.tsx
"use client";

import { useEffect, useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { api } from "@/lib/api";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { toast } from "sonner";

/* Backend’in beklediği alanlar */
const schema = z.object({
  appId: z.string().uuid({ message: "Bir uygulama seç." }),
  code: z.string().min(2, "Kod en az 2 karakter olmalı."),
  name: z.string().min(2, "Ad en az 2 karakter olmalı."),
  description: z.string().optional().or(z.literal("")),
});

type FormValues = z.infer<typeof schema>;

type AppLite = { id: string; name: string };
type Paged<T> = { items: T[]; page: number; pageSize: number; total: number };

export function FeatureForm({ onCreated }: { onCreated?: () => void }) {
  const [apps, setApps] = useState<AppLite[]>([]);
  const [loadingApps, setLoadingApps] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { appId: "", code: "", name: "", description: "" },
  });

  // App listesi: sade native select kullanıyoruz; Radix’le hydration kavgası yok
  useEffect(() => {
    let cancelled = false;
    (async () => {
      setLoadingApps(true);
      try {
        const res = await api.get<Paged<AppLite>>("/apps?page=1&pageSize=500");
        if (!cancelled) setApps(res.data.items ?? []);
      } catch {
        toast.error("Uygulamalar yüklenemedi");
      } finally {
        if (!cancelled) setLoadingApps(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, []);

  const hasApps = useMemo(() => apps.length > 0, [apps.length]);

  const onSubmit = async (values: FormValues) => {
    setSubmitting(true);
    try {
      await api.post("/features", values);
      toast.success("Özellik eklendi");
      reset({ appId: values.appId, code: "", name: "", description: "" }); // aynı app seçili kalsın
      onCreated?.();
    } catch (e: any) {
      const msg = e?.response?.data?.message || e?.message || "Kayıt başarısız";
      toast.error(msg);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <form className="grid gap-4" onSubmit={handleSubmit(onSubmit)}>
      {/* App seçimi */}
      <div className="grid gap-2">
        <Label htmlFor="appId">Uygulama</Label>
        <select
          id="appId"
          disabled={loadingApps || !hasApps}
          className="h-9 rounded-md border bg-background px-2 text-sm"
          {...register("appId")}
        >
          <option value="">{loadingApps ? "Yükleniyor..." : "Uygulama seç"}</option>
          {apps.map(a => (
            <option key={a.id} value={a.id}>
              {a.name}
            </option>
          ))}
        </select>
        {!hasApps && !loadingApps && (
          <p className="text-xs text-muted-foreground">
            Uygulama bulunamadı. Önce “Uygulamalar” sayfasından bir tane ekle.
          </p>
        )}
        {errors.appId && <p className="text-xs text-red-500">{errors.appId.message}</p>}
      </div>

      <div className="grid gap-2 md:grid-cols-2">
        <div className="grid gap-2">
          <Label htmlFor="code">Kod</Label>
          <Input id="code" placeholder="örn. audit.logs" {...register("code")} />
          {errors.code && <p className="text-xs text-red-500">{errors.code.message}</p>}
        </div>
        <div className="grid gap-2">
          <Label htmlFor="name">Ad</Label>
          <Input id="name" placeholder="örn. Denetim Logları" {...register("name")} />
          {errors.name && <p className="text-xs text-red-500">{errors.name.message}</p>}
        </div>
      </div>

      <div className="grid gap-2">
        <Label htmlFor="description">Açıklama</Label>
        <Textarea id="description" rows={3} placeholder="İsteğe bağlı" {...register("description")} />
      </div>

      <Button type="submit" disabled={submitting || !hasApps}>
        {submitting ? "Kaydediliyor..." : "Kaydet"}
      </Button>
    </form>
  );
}
