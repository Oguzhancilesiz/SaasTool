// src/app/(protected)/plans/page.tsx
"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { PlanForm } from "@/components/plan-form";

type PlanDto = {
  id: string;
  code: string;
  name: string;
  price: number;
  currency: number;       // 0/1/2
  billingPeriod: number;  // 0/1
  isPublic: boolean;
};

type PagedResponse<T> = {
  total: number;
  page: number;
  pageSize: number;
  items: T[];
};

export default function PlansPage() {
  const [data, setData] = useState<PagedResponse<PlanDto> | null>(null);
  const load = async () => {
    const { data } = await api.get<PagedResponse<PlanDto>>("/plans?page=1&pageSize=50");
    setData(data);
  };

  useEffect(() => { load(); }, []);

  return (
    <div className="grid gap-6">
      <Card>
        <CardHeader><CardTitle>Yeni Plan</CardTitle></CardHeader>
        <CardContent>
          <PlanForm onCreated={load} />
        </CardContent>
      </Card>

      <Card>
        <CardHeader><CardTitle>Planlar</CardTitle></CardHeader>
        <CardContent>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="text-left border-b">
                  <th className="py-2">Kod</th>
                  <th>Ad</th>
                  <th>Fiyat</th>
                  <th>Döviz</th>
                  <th>Dönem</th>
                  <th>Açık mı</th>
                </tr>
              </thead>
              <tbody>
                {data?.items.map(p => (
                  <tr key={p.id} className="border-b">
                    <td className="py-2">{p.code}</td>
                    <td>{p.name}</td>
                    <td>{p.price}</td>
                    <td>{["TRY","USD","EUR"][p.currency] ?? p.currency}</td>
                    <td>{["Aylık","Yıllık"][p.billingPeriod] ?? p.billingPeriod}</td>
                    <td>{p.isPublic ? "Evet" : "Hayır"}</td>
                  </tr>
                ))}
                {!data?.items.length && (
                  <tr><td className="py-4 text-muted-foreground" colSpan={6}>Kayıt yok</td></tr>
                )}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
