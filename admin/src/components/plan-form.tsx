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

const schema = z.object({
  code: z.string().min(2),
  name: z.string().min(2),
  description: z.string().optional(),
  currency: z.nativeEnum({ 0: "Try", 1: "Usd", 2: "Eur" } as any).or(z.number().int().min(0).max(2)),
  price: z.number().nonnegative(),
  billingPeriod: z.nativeEnum({ 0: "Monthly", 1: "Yearly" } as any).or(z.number().int().min(0).max(1)),
  isPublic: z.boolean().default(true),
  trialDays: z.number().int().nonnegative().max(365).default(0),
});
type FormValues = z.infer<typeof schema>;

export function PlanForm({ onCreated }: { onCreated: () => void }) {
  const { register, handleSubmit, formState: { isSubmitting, errors }, reset } =
    useForm<FormValues>({
      resolver: zodResolver(schema),
      defaultValues: { currency: 0 as any, billingPeriod: 0 as any, isPublic: true, trialDays: 0 },
    });

  const onSubmit = async (values: FormValues) => {
    try {
      // API enumları integer bekliyor; string gelirse sayıya çevir
      const payload = {
        ...values,
        currency: Number(values.currency),
        billingPeriod: Number(values.billingPeriod),
      };
      await api.post("/plans", payload);
      toast.success("Plan oluşturuldu");
      reset();
      onCreated();
    } catch (e: any) {
      toast.error("Plan oluşturulamadı");
      console.error(e?.response?.data || e);
    }
  };

  return (
    <form className="grid gap-3" onSubmit={handleSubmit(onSubmit)}>
      <div className="grid gap-1">
        <Label>Kod</Label>
        <Input {...register("code")} />
        {errors.code && <p className="text-sm text-red-500">{errors.code.message}</p>}
      </div>
      <div className="grid gap-1">
        <Label>Ad</Label>
        <Input {...register("name")} />
      </div>
      <div className="grid gap-1">
        <Label>Açıklama</Label>
        <Textarea rows={3} {...register("description")} />
      </div>

      <div className="grid grid-cols-3 gap-3">
        <div>
          <Label>Döviz (0=TRY,1=USD,2=EUR)</Label>
          <Input type="number" {...register("currency", { valueAsNumber: true })} />
        </div>
        <div>
          <Label>Fiyat</Label>
          <Input type="number" step="0.01" {...register("price", { valueAsNumber: true })} />
        </div>
        <div>
          <Label>Dönem (0=Aylık,1=Yıllık)</Label>
          <Input type="number" {...register("billingPeriod", { valueAsNumber: true })} />
        </div>
      </div>

      <div className="grid grid-cols-2 gap-3">
        <div>
          <Label>Trial (gün)</Label>
          <Input type="number" {...register("trialDays", { valueAsNumber: true })} />
        </div>
        <div className="flex items-end gap-2">
          <input id="isPublic" type="checkbox" {...register("isPublic")} />
          <Label htmlFor="isPublic">Herkese açık</Label>
        </div>
      </div>

      <Button disabled={isSubmitting} type="submit">
        {isSubmitting ? "Kaydediliyor..." : "Kaydet"}
      </Button>
    </form>
  );
}
