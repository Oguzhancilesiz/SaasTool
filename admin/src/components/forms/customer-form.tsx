// src/components/customer-form.tsx
"use client";

import { useEffect, useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";

import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";

import { api } from "@/lib/api";
import { toast } from "sonner";
import { useOrgStore } from "@/stores/org-store";

const schema = z.object({
  organizationId: z.string().uuid({ message: "Bir organizasyon seç." }),
  name: z.string().min(2, "Ad en az 2 karakter"),
  email: z.string().email("Geçerli bir e-posta gir"),
  taxNumber: z.string().optional().or(z.literal("")),
  billingAddress: z.string().optional().or(z.literal("")),
  country: z.string().optional().or(z.literal("")),
  city: z.string().optional().or(z.literal("")),
});
type FormValues = z.infer<typeof schema>;

type OrgLite = { id: string; name: string };
type Paged<T> = { items: T[]; page: number; pageSize: number; total: number };

export function CustomerForm({ onCreated }: { onCreated: () => void }) {
  // Seçili org varsa otomatik doldur
  const selectedOrgId = useOrgStore(s => s.orgId);

  const [orgs, setOrgs] = useState<OrgLite[]>([]);
  const [loadingOrgs, setLoadingOrgs] = useState(false);

  const {
    register,
    handleSubmit,
    reset,
    setValue,
    formState: { isSubmitting, errors },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      organizationId: selectedOrgId ?? "",
      name: "",
      email: "",
      taxNumber: "",
      billingAddress: "",
      country: "",
      city: "",
    },
  });

  // Organizasyonları getir
  useEffect(() => {
    let cancelled = false;
    (async () => {
      setLoadingOrgs(true);
      try {
        const res = await api.get<Paged<OrgLite>>("/organizations?page=1&pageSize=500");
        if (!cancelled) {
          const items = res.data.items ?? [];
          setOrgs(items);
          // store’daki org listede varsa seçili yap
          if (selectedOrgId && items.some(o => o.id === selectedOrgId)) {
            setValue("organizationId", selectedOrgId);
          }
        }
      } catch {
        // Dünya yanmasa da liste gelmeyebilir; form yine çalışsın.
      } finally {
        if (!cancelled) setLoadingOrgs(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [selectedOrgId, setValue]);

  const hasOrgs = useMemo(() => orgs.length > 0, [orgs.length]);

  const onSubmit = async (v: FormValues) => {
    try {
      await api.post("/customers", v);
      toast.success("Müşteri eklendi");
      // Org seçimi sabit kalsın, diğer alanları temizle
      reset({
        organizationId: v.organizationId,
        name: "",
        email: "",
        taxNumber: "",
        billingAddress: "",
        country: "",
        city: "",
      });
      onCreated();
    } catch (e: any) {
      toast.error(e?.response?.data?.message || "Kayıt başarısız");
    }
  };

  return (
    <form className="grid gap-4" onSubmit={handleSubmit(onSubmit)}>
      {/* Organizasyon */}
      <div className="grid gap-1">
        <Label htmlFor="organizationId">Organizasyon</Label>
        <select
          id="organizationId"
          className="h-9 rounded-md border bg-background px-2 text-sm"
          disabled={loadingOrgs || !hasOrgs}
          {...register("organizationId")}
        >
          <option value="">
            {loadingOrgs ? "Yükleniyor..." : hasOrgs ? "Organizasyon seç" : "Organizasyon bulunamadı"}
          </option>
          {orgs.map(o => (
            <option key={o.id} value={o.id}>
              {o.name}
            </option>
          ))}
        </select>
        {errors.organizationId && (
          <p className="text-xs text-red-500">{errors.organizationId.message}</p>
        )}
      </div>

      {/* Ad - Email */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
        <div className="grid gap-1">
          <Label htmlFor="name">Ad</Label>
          <Input id="name" placeholder="Acme Ltd." {...register("name")} />
          {errors.name && <p className="text-xs text-red-500">{errors.name.message}</p>}
        </div>
        <div className="grid gap-1">
          <Label htmlFor="email">E-posta</Label>
          <Input id="email" type="email" placeholder="billing@acme.co" {...register("email")} />
          {errors.email && <p className="text-xs text-red-500">{errors.email.message}</p>}
        </div>
      </div>

      {/* Vergi - Şehir */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
        <div className="grid gap-1">
          <Label htmlFor="taxNumber">Vergi No</Label>
          <Input id="taxNumber" placeholder="Opsiyonel" {...register("taxNumber")} />
        </div>
        <div className="grid gap-1">
          <Label htmlFor="city">Şehir</Label>
          <Input id="city" placeholder="İstanbul" {...register("city")} />
        </div>
      </div>

      {/* Ülke - Adres */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
        <div className="grid gap-1">
          <Label htmlFor="country">Ülke</Label>
          <Input id="country" placeholder="Türkiye" {...register("country")} />
        </div>
        <div className="grid gap-1">
          <Label htmlFor="billingAddress">Adres</Label>
          <Textarea id="billingAddress" rows={3} placeholder="Sokak, No, İlçe" {...register("billingAddress")} />
        </div>
      </div>

      <Button disabled={isSubmitting || !hasOrgs} type="submit">
        {isSubmitting ? "Kaydediliyor..." : "Kaydet"}
      </Button>

      {!hasOrgs && !loadingOrgs && (
        <p className="text-xs text-muted-foreground">
          Müşteri eklemek için önce bir organizasyon oluşturmalısın.
        </p>
      )}
    </form>
  );
}
