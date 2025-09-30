// src/components/forms/subscription-form.tsx
"use client";

import { useEffect, useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";

import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";

import { api } from "@/lib/api";
import { toast } from "sonner";

/* ---------- Zod şema ---------- */
const schema = z.object({
  organizationId: z.string().uuid({ message: "Bir organizasyon seç." }),
  appId: z.string().uuid({ message: "Bir uygulama seç." }),
  planId: z.string().uuid({ message: "Bir plan seç." }),
  customerId: z.string().uuid().optional().or(z.literal("")),
  startsAt: z.string().optional().or(z.literal("")),
  cancelAtPeriodEnd: z.boolean().optional().default(false),
});
type FormValues = z.infer<typeof schema>;

/* ---------- Basit tipler (kendini taşır, dış tipe bağlı değil) ---------- */
type Paged<T> = { items: T[]; page: number; pageSize: number; total: number };
type IdName = { id: string; name?: string; code?: string };

/* ---------- Yardımcılar ---------- */
function toIsoOrNow(s?: string | null) {
  if (!s) return new Date().toISOString();
  // datetime-local değerini ISO'ya çevir
  const d = new Date(s);
  return isNaN(d.getTime()) ? new Date().toISOString() : d.toISOString();
}

export function SubscriptionForm({ onCreated }: { onCreated: () => void }) {
  /* Form */
  const {
    register,
    handleSubmit,
    reset,
    watch,
    formState: { isSubmitting, errors },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      organizationId: "",
      appId: "",
      planId: "",
      customerId: "",
      startsAt: "",
      cancelAtPeriodEnd: false,
    },
  });

  const selectedOrgId = watch("organizationId");

  /* Referans listeler */
  const [orgs, setOrgs] = useState<IdName[]>([]);
  const [apps, setApps] = useState<IdName[]>([]);
  const [plans, setPlans] = useState<IdName[]>([]);
  const [customers, setCustomers] = useState<IdName[]>([]);
  const [loadingRefs, setLoadingRefs] = useState(false);
  const [loadingCustomers, setLoadingCustomers] = useState(false);

  // İlk yüklemede org/app/plan getir
  useEffect(() => {
    let cancelled = false;
    (async () => {
      setLoadingRefs(true);
      try {
        const [o, a, p] = await Promise.all([
          api.get<Paged<IdName>>("/organizations?page=1&pageSize=500"),
          api.get<Paged<IdName>>("/apps?page=1&pageSize=500"),
          api.get<Paged<IdName>>("/plans?page=1&pageSize=500"),
        ]);
        if (!cancelled) {
          setOrgs(o.data.items ?? []);
          setApps(a.data.items ?? []);
          setPlans(p.data.items ?? []);
        }
      } catch {
        // sessizce geç
      } finally {
        if (!cancelled) setLoadingRefs(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, []);

  // Organizasyon seçilince müşterileri getir (filtreli)
  useEffect(() => {
    let cancelled = false;
    (async () => {
      if (!selectedOrgId) {
        setCustomers([]);
        return;
      }
      setLoadingCustomers(true);
      try {
        const res = await api.get<Paged<IdName>>(
          `/customers?page=1&pageSize=500&organizationId=${encodeURIComponent(selectedOrgId)}`
        );
        if (!cancelled) setCustomers(res.data.items ?? []);
      } catch {
        if (!cancelled) setCustomers([]);
      } finally {
        if (!cancelled) setLoadingCustomers(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [selectedOrgId]);

  const hasOrgs = useMemo(() => orgs.length > 0, [orgs.length]);
  const hasApps = useMemo(() => apps.length > 0, [apps.length]);
  const hasPlans = useMemo(() => plans.length > 0, [plans.length]);
  const hasCustomers = useMemo(() => customers.length > 0, [customers.length]);

  /* Submit */
  const onSubmit = async (v: FormValues) => {
    const payload = {
      ...v,
      startsAt: toIsoOrNow(v.startsAt),
      // Backend bekliyor diye boş da olsa gönderiyoruz
      items: [] as any[],
      // customerId boş string ise gönderme
      ...(v.customerId ? { customerId: v.customerId } : { customerId: undefined }),
    };

    try {
      await api.post("/subscriptions", payload);
      toast.success("Abonelik oluşturuldu");
      // Seçimler kalsın, tarih/opsiyon sıfırlansın
      reset({
        organizationId: v.organizationId,
        appId: v.appId,
        planId: v.planId,
        customerId: v.customerId ?? "",
        startsAt: "",
        cancelAtPeriodEnd: false,
      });
      onCreated();
    } catch (e: any) {
      toast.error(e?.response?.data?.message || "Kayıt başarısız");
    }
  };

  /* UI */
  return (
    <form className="grid gap-4" onSubmit={handleSubmit(onSubmit)}>
      {/* Satır 1: Org - App */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
        <div className="grid gap-1">
          <Label htmlFor="organizationId">Organizasyon</Label>
          <select
            id="organizationId"
            className="h-9 rounded-md border bg-background px-2 text-sm"
            disabled={loadingRefs || !hasOrgs}
            {...register("organizationId")}
          >
            <option value="">{loadingRefs ? "Yükleniyor..." : hasOrgs ? "Seç" : "Organizasyon yok"}</option>
            {orgs.map(o => (
              <option key={o.id} value={o.id}>
                {o.name || o.code || o.id}
              </option>
            ))}
          </select>
          {errors.organizationId && (
            <p className="text-xs text-red-500">{errors.organizationId.message}</p>
          )}
        </div>

        <div className="grid gap-1">
          <Label htmlFor="appId">Uygulama</Label>
          <select
            id="appId"
            className="h-9 rounded-md border bg-background px-2 text-sm"
            disabled={loadingRefs || !hasApps}
            {...register("appId")}
          >
            <option value="">{loadingRefs ? "Yükleniyor..." : hasApps ? "Seç" : "Uygulama yok"}</option>
            {apps.map(a => (
              <option key={a.id} value={a.id}>
                {a.name || a.code || a.id}
              </option>
            ))}
          </select>
          {errors.appId && <p className="text-xs text-red-500">{errors.appId.message}</p>}
        </div>
      </div>

      {/* Satır 2: Plan - Müşteri */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
        <div className="grid gap-1">
          <Label htmlFor="planId">Plan</Label>
          <select
            id="planId"
            className="h-9 rounded-md border bg-background px-2 text-sm"
            disabled={loadingRefs || !hasPlans}
            {...register("planId")}
          >
            <option value="">{loadingRefs ? "Yükleniyor..." : hasPlans ? "Seç" : "Plan yok"}</option>
            {plans.map(p => (
              <option key={p.id} value={p.id}>
                {p.name || p.code || p.id}
              </option>
            ))}
          </select>
          {errors.planId && <p className="text-xs text-red-500">{errors.planId.message}</p>}
        </div>

        <div className="grid gap-1">
          <Label htmlFor="customerId">Müşteri (opsiyonel)</Label>
          <select
            id="customerId"
            className="h-9 rounded-md border bg-background px-2 text-sm"
            disabled={!selectedOrgId || loadingCustomers || !hasCustomers}
            {...register("customerId")}
          >
            <option value="">
              {!selectedOrgId
                ? "Önce organizasyon seç"
                : loadingCustomers
                ? "Yükleniyor..."
                : hasCustomers
                ? "Seç"
                : "Müşteri yok"}
            </option>
            {customers.map(c => (
              <option key={c.id} value={c.id}>
                {c.name || c.id}
              </option>
            ))}
          </select>
        </div>
      </div>

      {/* Satır 3: Başlangıç - İptal checkbox */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
        <div className="grid gap-1">
          <Label htmlFor="startsAt">Başlangıç</Label>
          <Input id="startsAt" type="datetime-local" {...register("startsAt")} />
          {/* Hata gerekmiyor; boşsa otomatik "şimdi" kullanıyoruz */}
        </div>

        <div className="flex items-end gap-2">
          <input id="cancelAtPeriodEnd" type="checkbox" className="h-4 w-4 accent-primary" {...register("cancelAtPeriodEnd")} />
          <Label htmlFor="cancelAtPeriodEnd">Dönem sonunda iptal</Label>
        </div>
      </div>

      <Button disabled={isSubmitting || loadingRefs} type="submit">
        {isSubmitting ? "Kaydediliyor..." : "Kaydet"}
      </Button>
    </form>
  );
}
