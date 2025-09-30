// src/components/forms/invoice-form.tsx
"use client";

import { useEffect, useMemo, useState } from "react";
import { useForm, useFieldArray } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";

import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { api } from "@/lib/api";
import { toast } from "sonner";

/* ------------ Şema ------------ */
const lineSchema = z.object({
  description: z.string().min(1, "Açıklama gerekli"),
  quantity: z.number({ invalid_type_error: "Adet sayı olmalı" }).positive("Adet > 0 olmalı"),
  unitPrice: z.number({ invalid_type_error: "Birim fiyat sayı olmalı" }).nonnegative("Negatif olamaz"),
});

const schema = z.object({
  organizationId: z.string().uuid({ message: "Organizasyon seç" }),
  customerId: z.string().uuid().optional().or(z.literal("")),
  currency: z.number().int().min(0),
  dueDate: z.string().optional().or(z.literal("")),
  provider: z.number().int().min(0),
  lines: z.array(lineSchema).min(1, "En az 1 satır ekle"),
});

type FormValues = z.infer<typeof schema>;

type Paged<T> = { items: T[]; page: number; pageSize: number; total: number };
type IdName = { id: string; name?: string; code?: string };

/* ------------ Seçenekler ------------ */
const currencyOptions = [
  { value: 0, label: "TRY" },
  { value: 1, label: "USD" },
  { value: 2, label: "EUR" },
];

// Backend’inde provider enum sırası neyse ona göre düzeltirsin.
const providerOptions = [
  { value: 0, label: "Internal" },
  { value: 1, label: "Stripe" },
  { value: 2, label: "Paddle" },
];

/* ------------ Yardımcılar ------------ */
const toIsoOrNull = (s?: string | null) => (s ? new Date(s).toISOString() : null);

export function InvoiceForm({ onCreated }: { onCreated: () => void }) {
  const {
    register,
    handleSubmit,
    reset,
    control,
    watch,
    setValue,
    formState: { isSubmitting, errors },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      organizationId: "",
      customerId: "",
      currency: 0,
      dueDate: "",
      provider: 0,
      lines: [{ description: "Satır", quantity: 1, unitPrice: 0 }],
    },
  });

  const { fields, append, remove } = useFieldArray({ control, name: "lines" });

  const selectedOrgId = watch("organizationId");
  const currency = watch("currency");
  const lines = watch("lines");

  // Referans listeler
  const [orgs, setOrgs] = useState<IdName[]>([]);
  const [customers, setCustomers] = useState<IdName[]>([]);
  const [loadingRefs, setLoadingRefs] = useState(false);
  const [loadingCustomers, setLoadingCustomers] = useState(false);

  /* ---- İlk yüklemede organizasyonları getir ---- */
  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        setLoadingRefs(true);
        const res = await api.get<Paged<IdName>>("/organizations?page=1&pageSize=500");
        if (!cancelled) setOrgs(res.data.items ?? []);
      } catch {
        // geç
      } finally {
        if (!cancelled) setLoadingRefs(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, []);

  /* ---- Org seçilince müşterileri getir ---- */
  useEffect(() => {
    let cancelled = false;
    (async () => {
      if (!selectedOrgId) {
        setCustomers([]);
        setValue("customerId", "");
        return;
      }
      try {
        setLoadingCustomers(true);
        const res = await api.get<Paged<IdName>>(
          `/customers?page=1&pageSize=500&organizationId=${encodeURIComponent(selectedOrgId)}`
        );
        if (!cancelled) {
          setCustomers(res.data.items ?? []);
          // mevcut seçili müşteri başka org’a aitse sıfırla
          setValue("customerId", v => {
            const ok = res.data.items?.some(c => c.id === v);
            return ok ? v : "";
          });
        }
      } catch {
        if (!cancelled) setCustomers([]);
      } finally {
        if (!cancelled) setLoadingCustomers(false);
      }
    })();
    return () => {
      cancelled = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedOrgId]);

  /* ---- Toplamlar ---- */
  const subTotal = useMemo(() => {
    return (lines ?? []).reduce((sum, l) => sum + (Number(l.quantity) || 0) * (Number(l.unitPrice) || 0), 0);
  }, [lines]);
  const currencyLabel = useMemo(() => currencyOptions.find(c => c.value === currency)?.label ?? "", [currency]);

  /* ---- Submit ---- */
  const onSubmit = async (v: FormValues) => {
    const payload = {
      ...v,
      // boş string ise null
      customerId: v.customerId || undefined,
      dueDate: toIsoOrNull(v.dueDate),
    };

    try {
      await api.post("/invoices", payload);
      toast.success("Fatura oluşturuldu");
      reset({
        organizationId: v.organizationId, // org/müşteri kalsın istersen değiştir
        customerId: v.customerId ?? "",
        currency: v.currency,
        dueDate: "",
        provider: v.provider,
        lines: [{ description: "Satır", quantity: 1, unitPrice: 0 }],
      });
      onCreated();
    } catch (e: any) {
      toast.error(e?.response?.data?.message || "Kayıt başarısız");
    }
  };

  /* ---- UI ---- */
  return (
    <form className="grid gap-4" onSubmit={handleSubmit(onSubmit)}>
      {/* Satır 1: Org - Müşteri */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
        <div className="grid gap-1">
          <Label htmlFor="organizationId">Organizasyon</Label>
          <select
            id="organizationId"
            className="h-9 rounded-md border bg-background px-2 text-sm"
            disabled={loadingRefs || orgs.length === 0}
            {...register("organizationId")}
          >
            <option value="">{loadingRefs ? "Yükleniyor..." : orgs.length ? "Seç" : "Organizasyon yok"}</option>
            {orgs.map(o => (
              <option key={o.id} value={o.id}>
                {o.name || o.code || o.id}
              </option>
            ))}
          </select>
          {errors.organizationId && <p className="text-xs text-red-500">{errors.organizationId.message}</p>}
        </div>

        <div className="grid gap-1">
          <Label htmlFor="customerId">Müşteri (opsiyonel)</Label>
          <select
            id="customerId"
            className="h-9 rounded-md border bg-background px-2 text-sm"
            disabled={!selectedOrgId || loadingCustomers}
            {...register("customerId")}
          >
            <option value="">
              {!selectedOrgId ? "Önce organizasyon seç" : loadingCustomers ? "Yükleniyor..." : "Seç"}
            </option>
            {customers.map(c => (
              <option key={c.id} value={c.id}>
                {c.name || c.id}
              </option>
            ))}
          </select>
        </div>
      </div>

      {/* Satır 2: Döviz - Vade - Sağlayıcı */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
        <div className="grid gap-1">
          <Label htmlFor="currency">Döviz</Label>
          <select
            id="currency"
            className="h-9 rounded-md border bg-background px-2 text-sm"
            {...register("currency", { valueAsNumber: true })}
          >
            {currencyOptions.map(c => (
              <option key={c.value} value={c.value}>
                {c.label}
              </option>
            ))}
          </select>
        </div>

        <div className="grid gap-1">
          <Label htmlFor="dueDate">Vade</Label>
          <Input id="dueDate" type="datetime-local" {...register("dueDate")} />
        </div>

        <div className="grid gap-1">
          <Label htmlFor="provider">Sağlayıcı</Label>
          <select
            id="provider"
            className="h-9 rounded-md border bg-background px-2 text-sm"
            {...register("provider", { valueAsNumber: true })}
          >
            {providerOptions.map(p => (
              <option key={p.value} value={p.value}>
                {p.label}
              </option>
            ))}
          </select>
        </div>
      </div>

      {/* Satırlar */}
      <div className="rounded-xl border p-3">
        <div className="flex items-center justify-between mb-2">
          <div className="text-sm font-medium">Satırlar</div>
          <Button
            type="button"
            variant="outline"
            size="sm"
            onClick={() => append({ description: "Satır", quantity: 1, unitPrice: 0 })}
          >
            Satır ekle
          </Button>
        </div>

        {fields.map((f, idx) => {
          const q = Number(lines?.[idx]?.quantity || 0);
          const up = Number(lines?.[idx]?.unitPrice || 0);
          const lineTotal = q * up;

          return (
            <div key={f.id} className="grid grid-cols-1 md:grid-cols-12 gap-2 items-end mb-2">
              <div className="md:col-span-6">
                <Label>Açıklama</Label>
                <Input {...register(`lines.${idx}.description` as const)} />
                {errors.lines?.[idx]?.description && (
                  <p className="text-xs text-red-500">
                    {errors.lines[idx]?.description?.message as string}
                  </p>
                )}
              </div>

              <div className="md:col-span-2">
                <Label>Adet</Label>
                <Input
                  type="number"
                  step="1"
                  {...register(`lines.${idx}.quantity` as const, { valueAsNumber: true })}
                />
                {errors.lines?.[idx]?.quantity && (
                  <p className="text-xs text-red-500">
                    {errors.lines[idx]?.quantity?.message as string}
                  </p>
                )}
              </div>

              <div className="md:col-span-2">
                <Label>Birim Fiyat</Label>
                <Input
                  type="number"
                  step="0.01"
                  {...register(`lines.${idx}.unitPrice` as const, { valueAsNumber: true })}
                />
                {errors.lines?.[idx]?.unitPrice && (
                  <p className="text-xs text-red-500">
                    {errors.lines[idx]?.unitPrice?.message as string}
                  </p>
                )}
              </div>

              <div className="md:col-span-2">
                <Label>Ara Toplam</Label>
                <div className="h-9 flex items-center px-3 rounded-md border bg-muted/40 text-sm">
                  {lineTotal.toFixed(2)} {currencyLabel}
                </div>
              </div>

              <div className="md:col-span-12">
                <button
                  type="button"
                  className="text-xs text-red-600 hover:underline"
                  onClick={() => remove(idx)}
                >
                  Satırı sil
                </button>
              </div>
            </div>
          );
        })}

        {typeof errors.lines?.message === "string" && (
          <p className="text-xs text-red-500">{errors.lines?.message as string}</p>
        )}
      </div>

      {/* Toplam */}
      <div className="flex items-center justify-end gap-4">
        <div className="text-sm text-muted-foreground">Genel Toplam</div>
        <div className="text-base font-semibold">
          {subTotal.toFixed(2)} {currencyLabel}
        </div>
      </div>

      <Button disabled={isSubmitting} type="submit">
        {isSubmitting ? "Kaydediliyor..." : "Kaydet"}
      </Button>
    </form>
  );
}
