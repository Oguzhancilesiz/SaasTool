// src/components/plan-form.tsx
"use client";

import { z } from "zod";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { api } from "@/lib/api";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { useMemo } from "react";

const CURRENCIES = ["TRY", "USD", "EUR"] as const;          // 0,1,2
const PERIODS = ["Aylık", "Yıllık"] as const;               // 0,1

const schema = z.object({
  code: z.string().min(2, "Kod en az 2 karakter"),
  name: z.string().min(2, "Ad en az 2 karakter"),
  description: z.string().optional().or(z.literal("")),
  currency: z.union([
    z.enum(["0", "1", "2"]),
    z.number().int().min(0).max(2)
  ]),
  price: z.number({ invalid_type_error: "Fiyat sayı olmalı" }).nonnegative("Eksi fiyat olmaz"),
  billingPeriod: z.union([
    z.enum(["0", "1"]),
    z.number().int().min(0).max(1)
  ]),
  isPublic: z.boolean().default(true),
  trialDays: z
    .number({ invalid_type_error: "Gün sayı olmalı" })
    .int("Tam sayı olmalı")
    .min(0, "Negatif olamaz")
    .max(365, "365 günden fazla olamaz")
    .default(0),
});
type FormValues = z.infer<typeof schema>;

export function PlanForm({ onCreated }: { onCreated: () => void }) {
  const {
    register,
    handleSubmit,
    formState: { isSubmitting, errors },
    reset,
    watch,
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      currency: "0",
      billingPeriod: "0",
      isPublic: true,
      trialDays: 0,
    },
  });

  const currencyIdx = Number(watch("currency") ?? 0);
  const currencyLabel = useMemo(
    () => CURRENCIES[currencyIdx] ?? "TRY",
    [currencyIdx]
  );

  const onSubmit = async (values: FormValues) => {
    try {
      // enumları API’nin beklediği integer formuna çevir
      const payload = {
        ...values,
        currency: Number(values.currency),
        billingPeriod: Number(values.billingPeriod),
      };
      await api.post("/plans", payload);
      toast.success("Plan oluşturuldu");
      // seçilen para birimi ve dönem kalsın, diğerlerini temizle
      reset({
        currency: String(payload.currency) as any,
        billingPeriod: String(payload.billingPeriod) as any,
        isPublic: true,
        trialDays: 0,
        code: "",
        name: "",
        description: "",
        price: undefined as any,
      });
      onCreated();
    } catch (e: any) {
      toast.error("Plan oluşturulamadı");
      console.error(e?.response?.data || e);
    }
  };

  return (
    <form className="grid gap-4" onSubmit={handleSubmit(onSubmit)}>
      <div className="grid gap-1">
        <Label htmlFor="code">Kod</Label>
        <Input id="code" placeholder="örn. pro" {...register("code")} />
        {errors.code && <p className="text-sm text-red-500">{errors.code.message}</p>}
      </div>

      <div className="grid gap-1">
        <Label htmlFor="name">Ad</Label>
        <Input id="name" placeholder="örn. Pro Plan" {...register("name")} />
        {errors.name && <p className="text-sm text-red-500">{errors.name.message}</p>}
      </div>

      <div className="grid gap-1">
        <Label htmlFor="description">Açıklama</Label>
        <Textarea id="description" rows={3} placeholder="İsteğe bağlı" {...register("description")} />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
        {/* Döviz */}
        <div className="grid gap-1">
          <Label htmlFor="currency">Döviz</Label>
          <select
            id="currency"
            className="h-9 rounded-md border bg-background px-2 text-sm"
            {...register("currency")}
          >
            {CURRENCIES.map((c, i) => (
              <option key={c} value={i}>{c}</option>
            ))}
          </select>
        </div>

        {/* Fiyat */}
        <div className="grid gap-1">
          <Label htmlFor="price">Fiyat</Label>
          <div className="relative">
            <span className="absolute left-2 top-1/2 -translate-y-1/2 text-xs text-muted-foreground">
              {currencyLabel}
            </span>
            <Input
              id="price"
              type="number"
              step="0.01"
              inputMode="decimal"
              className="pl-12"
              {...register("price", { valueAsNumber: true })}
              placeholder="0.00"
            />
          </div>
          {errors.price && <p className="text-sm text-red-500">{errors.price.message}</p>}
        </div>

        {/* Dönem */}
        <div className="grid gap-1">
          <Label htmlFor="billingPeriod">Dönem</Label>
          <select
            id="billingPeriod"
            className="h-9 rounded-md border bg-background px-2 text-sm"
            {...register("billingPeriod")}
          >
            {PERIODS.map((p, i) => (
              <option key={p} value={i}>{p}</option>
            ))}
          </select>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
        {/* Trial */}
        <div className="grid gap-1">
          <Label htmlFor="trialDays">Trial (gün)</Label>
          <Input
            id="trialDays"
            type="number"
            min={0}
            max={365}
            {...register("trialDays", { valueAsNumber: true })}
          />
          {errors.trialDays && <p className="text-sm text-red-500">{errors.trialDays.message}</p>}
        </div>

        {/* Herkese açık */}
        <div className="grid gap-1 md:col-span-2">
          <Label htmlFor="isPublic">Görünürlük</Label>
          <label className="inline-flex items-center gap-2 text-sm">
            <input id="isPublic" type="checkbox" className="h-4 w-4 accent-primary" {...register("isPublic")} />
            <span>Herkese açık</span>
          </label>
        </div>
      </div>

      <Button disabled={isSubmitting} type="submit">
        {isSubmitting ? "Kaydediliyor..." : "Kaydet"}
      </Button>
    </form>
  );
}
