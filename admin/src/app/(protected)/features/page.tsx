// src/app/(protected)/features/page.tsx
"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import { api } from "@/lib/api";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { ListToolbar } from "@/components/list-toolbar";
import { Pagination } from "@/components/pagination";
import { ConfirmButton } from "@/components/confirm-button";
import { toCsv, downloadCsv } from "@/lib/csv";
import { FeatureForm } from "@/components/forms/feature-form";
import { toast } from "sonner";
import { FeatureDto, Paged, AppDto } from "@/types/dto";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";

export default function FeaturesPage() {
  const [page, setPage] = useState(1);
  const pageSize = 20;

  // Arama + filtre
  const [searchTerm, setSearchTerm] = useState("");
  const [q, setQ] = useState("");
  const [appId, setAppId] = useState<string>("");

  const [data, setData] = useState<Paged<FeatureDto> | null>(null);
  const [apps, setApps] = useState<AppDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [err, setErr] = useState<string | null>(null);

  const abortRef = useRef<AbortController | null>(null);

  // Debounce arama
  useEffect(() => {
    const t = setTimeout(() => {
      setPage(1);
      setQ(searchTerm.trim());
    }, 350);
    return () => clearTimeout(t);
  }, [searchTerm]);

  // App listesi (id->name için)
  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        // hafif uçtan bir liste çekelim
        const res = await api.get<Paged<AppDto>>("/apps?page=1&pageSize=500");
        if (!cancelled) setApps(res.data.items ?? []);
      } catch {
        // sorun değil; isim bulamazsak id kısaltması gösteririz
      }
    })();
    return () => {
      cancelled = true;
    };
  }, []);

  const load = async () => {
    abortRef.current?.abort();
    const controller = new AbortController();
    abortRef.current = controller;

    setLoading(true);
    setErr(null);
    try {
      const p: Record<string, string> = {
        page: String(page),
        pageSize: String(pageSize),
      };
      if (q) p.search = q;
      if (appId) p.appId = appId;

      const qs = new URLSearchParams(p).toString();
      const res = await api.get<Paged<FeatureDto>>(`/features?${qs}`, {
        signal: controller.signal as AbortSignal,
      });
      setData(res.data);
    } catch (e: any) {
      if (e?.name === "CanceledError" || e?.code === "ERR_CANCELED") {
        // iptal
      } else {
        const msg = e?.response?.data?.message || e?.message || "Liste yüklenemedi.";
        setErr(msg);
        toast.error("Özellikler yüklenemedi");
      }
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load().catch(() => {});
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [page, q, appId]);

  const del = async (id: string) => {
    await api.delete(`/features/${id}`);
    toast.success("Silindi");
    await load();
  };

  // id -> isim
  const appName = (id: string | null | undefined) => {
    if (!id) return "-";
    const a = apps.find(x => x.id === id);
    if (a?.name) return a.name;
    // fallback: id kısalt
    return id.slice(0, 8) + "…" + id.slice(-4);
    };

  const exportCsv = () => {
    const rows = (data?.items ?? []).map(x => ({
      Uygulama: appName(x.appId),
      Kod: x.code,
      Ad: x.name,
    }));
    if (!rows.length) return toast.info("Kayıt yok");
    downloadCsv("features.csv", toCsv(rows));
  };

  const items = useMemo(() => data?.items ?? [], [data?.items]);

  return (
    <div className="grid gap-6">
      {/* Create */}
      <Card>
        <CardHeader>
          <CardTitle>Yeni Özellik</CardTitle>
        </CardHeader>
        <CardContent>
          <FeatureForm onCreated={load} />
        </CardContent>
      </Card>

      {/* List */}
      <Card>
        <CardHeader>
          <div className="flex flex-col gap-2 sm:flex-row sm:items-end sm:justify-between">
            <div>
              <CardTitle>Özellikler</CardTitle>
              {err && <p className="text-xs text-red-500 mt-1">{err}</p>}
            </div>
            <div className="mt-3 sm:mt-0 flex flex-col sm:flex-row gap-2 sm:items-center">
              {/* App filtresi (native select, paket derdi yok) */}
              <div className="flex items-center gap-2">
                <label htmlFor="app-filter" className="text-xs text-muted-foreground whitespace-nowrap">
                  Uygulama
                </label>
                <select
                  id="app-filter"
                  value={appId}
                  onChange={e => { setPage(1); setAppId(e.target.value); }}
                  className="h-9 rounded-md border bg-background px-2 text-sm"
                >
                  <option value="">Tümü</option>
                  {apps.map(a => (
                    <option key={a.id} value={a.id}>{a.name}</option>
                  ))}
                </select>
              </div>

              <ListToolbar
                onSearch={setSearchTerm}
                onRefresh={load}
                onExport={exportCsv}
              />
            </div>
          </div>
        </CardHeader>

        <CardContent>
          {/* Desktop tablo */}
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="sticky top-0 z-10 bg-background">
                <tr className="border-b text-left">
                  <th className="py-2">Uygulama</th>
                  <th>Kod</th>
                  <th>Ad</th>
                  <th className="w-32 text-right">İşlem</th>
                </tr>
              </thead>
              <tbody>
                {/* Loading skeleton */}
                {loading &&
                  Array.from({ length: 6 }).map((_, i) => (
                    <tr key={`sk-${i}`} className="border-b animate-pulse">
                      <td className="py-2"><div className="h-4 w-36 rounded bg-muted" /></td>
                      <td><div className="h-4 w-28 rounded bg-muted" /></td>
                      <td><div className="h-4 w-44 rounded bg-muted" /></td>
                      <td className="text-right"><div className="h-7 w-16 rounded bg-muted ml-auto" /></td>
                    </tr>
                  ))}

                {/* Rows */}
                {!loading &&
                  items.map(x => (
                    <tr key={x.id} className="border-b hover:bg-muted/30">
                      <td className="py-2 align-middle">
                        <div className="inline-flex items-center gap-2">
                          <Badge variant="outline">{appName(x.appId)}</Badge>
                        </div>
                      </td>
                      <td className="align-middle">
                        <code className="rounded bg-muted px-1.5 py-0.5 text-xs">{x.code}</code>
                      </td>
                      <td className="align-middle">{x.name}</td>
                      <td className="text-right align-middle">
                        <ConfirmButton onConfirm={() => del(x.id)}>
                          Sil
                        </ConfirmButton>
                      </td>
                    </tr>
                  ))}

                {/* Empty */}
                {!loading && !items.length && (
                  <tr>
                    <td colSpan={4} className="py-6 text-center text-muted-foreground">
                      Kayıt yok
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>

          {/* Mobil kart görünümü */}
          <div className="mt-4 grid gap-3 sm:hidden">
            {loading &&
              Array.from({ length: 4 }).map((_, i) => (
                <div key={`skm-${i}`} className="h-24 rounded-xl border animate-pulse bg-muted/40" />
              ))}
            {!loading &&
              items.map(x => (
                <div key={x.id} className="rounded-xl border p-3">
                  <div className="flex items-start justify-between gap-2">
                    <div>
                      <div className="text-sm font-medium">{x.name}</div>
                      <div className="text-xs text-muted-foreground mt-0.5">
                        Uygulama: <Badge variant="outline">{appName(x.appId)}</Badge>
                      </div>
                      <div className="mt-1">
                        <code className="rounded bg-muted px-1.5 py-0.5 text-xs">{x.code}</code>
                      </div>
                    </div>
                    <div className="flex flex-col gap-2">
                      <ConfirmButton onConfirm={() => del(x.id)}>Sil</ConfirmButton>
                    </div>
                  </div>
                </div>
              ))}
            {!loading && !items.length && (
              <div className="rounded-xl border p-6 text-center text-muted-foreground">
                Kayıt yok
              </div>
            )}
          </div>

          <div className="mt-3">
            <Pagination
              page={data?.page ?? page}
              pageSize={data?.pageSize ?? pageSize}
              total={data?.total ?? 0}
              onPage={setPage}
            />
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
