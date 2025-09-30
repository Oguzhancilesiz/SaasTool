// src/app/(protected)/plans/page.tsx
"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import { api } from "@/lib/api";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";
import { ListToolbar } from "@/components/list-toolbar";
import { Pagination } from "@/components/pagination";
import { toCsv, downloadCsv } from "@/lib/csv";
import { toast } from "sonner";
import { PlanForm } from "@/components/plan-form";

type PlanDto = {
  id: string;
  code: string;
  name: string;
  price: number;
  currency: number;       // 0/1/2  -> TRY/USD/EUR
  billingPeriod: number;  // 0/1    -> Aylık/Yıllık
  isPublic: boolean;
};

type PagedResponse<T> = {
  total: number;
  page: number;
  pageSize: number;
  items: T[];
};

type SortKey = "code" | "name" | "price" | "currency" | "billingPeriod" | "isPublic";
type SortDir = "asc" | "desc";

const CURRENCY = ["TRY", "USD", "EUR"] as const;
const PERIOD = ["Aylık", "Yıllık"] as const;

export default function PlansPage() {
  const [page, setPage] = useState(1);
  const pageSize = 20;

  // Arama: toolbar -> debounce -> q
  const [searchTerm, setSearchTerm] = useState("");
  const [q, setQ] = useState("");

  const [data, setData] = useState<PagedResponse<PlanDto> | null>(null);
  const [loading, setLoading] = useState(false);
  const [err, setErr] = useState<string | null>(null);

  const [sortKey, setSortKey] = useState<SortKey>("name");
  const [sortDir, setSortDir] = useState<SortDir>("asc");

  const abortRef = useRef<AbortController | null>(null);

  useEffect(() => {
    const t = setTimeout(() => {
      setPage(1);
      setQ(searchTerm.trim());
    }, 350);
    return () => clearTimeout(t);
  }, [searchTerm]);

  const load = async () => {
    abortRef.current?.abort();
    const controller = new AbortController();
    abortRef.current = controller;

    setLoading(true);
    setErr(null);
    try {
      const params = new URLSearchParams({
        page: String(page),
        pageSize: String(pageSize),
        search: q,
      });
      const res = await api.get<PagedResponse<PlanDto>>(`/plans?${params}`, {
        signal: controller.signal as AbortSignal,
      });
      setData(res.data);
    } catch (e: any) {
      if (e?.name === "CanceledError" || e?.code === "ERR_CANCELED") {
        // iptal edildi
      } else {
        const msg = e?.response?.data?.message || e?.message || "Liste yüklenemedi.";
        setErr(msg);
        toast.error("Planlar yüklenemedi");
      }
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    // sayfa + q değişince getir
    load().catch(() => {});
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [page, q]);

  // Sayfa içi sıralama
  const items = useMemo(() => {
    const arr = [...(data?.items ?? [])];
    arr.sort((a, b) => {
      const va =
        sortKey === "isPublic" || sortKey === "currency" || sortKey === "billingPeriod"
          ? Number((a as any)[sortKey])
          : sortKey === "price"
          ? Number(a.price)
          : String((a as any)[sortKey] ?? "").toLowerCase();

      const vb =
        sortKey === "isPublic" || sortKey === "currency" || sortKey === "billingPeriod"
          ? Number((b as any)[sortKey])
          : sortKey === "price"
          ? Number(b.price)
          : String((b as any)[sortKey] ?? "").toLowerCase();

      // @ts-ignore
      if (va < vb) return sortDir === "asc" ? -1 : 1;
      // @ts-ignore
      if (va > vb) return sortDir === "asc" ? 1 : -1;
      return 0;
    });
    return arr;
  }, [data?.items, sortKey, sortDir]);

  // Sıralama başlık tıklaması
  const onSort = (key: SortKey) => {
    setSortKey(prev => {
      if (prev !== key) {
        setSortDir("asc");
        return key;
      }
      setSortDir(d => (d === "asc" ? "desc" : "asc"));
      return key;
    });
  };

  // isPublic hızlı toggle (optimistic)
  const togglePublic = async (id: string, next: boolean) => {
    try {
      setData(d =>
        d
          ? { ...d, items: d.items.map(p => (p.id === id ? { ...p, isPublic: next } : p)) }
          : d
      );
      await api.patch(`/plans/${id}`, { isPublic: next });
      toast.success(next ? "Plan herkese açık" : "Plan gizlendi");
    } catch {
      setData(d =>
        d
          ? { ...d, items: d.items.map(p => (p.id === id ? { ...p, isPublic: !next } : p)) }
          : d
      );
      toast.error("Güncelleme başarısız");
    }
  };

  // CSV export (filtrelenmiş sayfa verisi)
  const exportCsv = () => {
    const rows = (items ?? []).map(p => ({
      Kod: p.code,
      Ad: p.name,
      Fiyat: p.price,
      Döviz: CURRENCY[p.currency] ?? p.currency,
      Dönem: PERIOD[p.billingPeriod] ?? p.billingPeriod,
      "Açık mı": p.isPublic ? "Evet" : "Hayır",
    }));
    if (!rows.length) return toast.info("İhracat için kayıt yok");
    downloadCsv("plans.csv", toCsv(rows));
  };

  const fmtPrice = (p: number, c: number) => {
    const code = CURRENCY[c] ?? "TRY";
    // TRY sembolünü düzgün gösterelim
    const locale = code === "TRY" ? "tr-TR" : code === "EUR" ? "de-DE" : "en-US";
    try {
      return new Intl.NumberFormat(locale, { style: "currency", currency: code }).format(p);
    } catch {
      return `${p.toFixed(2)} ${code}`;
    }
  };

  return (
    <div className="grid gap-6">
      {/* Create */}
      <Card>
        <CardHeader>
          <CardTitle>Yeni Plan</CardTitle>
        </CardHeader>
        <CardContent>
          <PlanForm onCreated={load} />
        </CardContent>
      </Card>

      {/* List */}
      <Card>
        <CardHeader>
          <div className="flex flex-col gap-2 sm:flex-row sm:items-end sm:justify-between">
            <div>
              <CardTitle>Planlar</CardTitle>
              {err && <p className="text-xs text-red-500 mt-1">{err}</p>}
            </div>
            <div className="mt-3 sm:mt-0">
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
                <tr className="text-left border-b">
                  <Th label="Kod" active={sortKey === "code"} dir={sortDir} onClick={() => onSort("code")} />
                  <Th label="Ad" active={sortKey === "name"} dir={sortDir} onClick={() => onSort("name")} />
                  <Th label="Fiyat" active={sortKey === "price"} dir={sortDir} onClick={() => onSort("price")} />
                  <Th label="Döviz" active={sortKey === "currency"} dir={sortDir} onClick={() => onSort("currency")} />
                  <Th label="Dönem" active={sortKey === "billingPeriod"} dir={sortDir} onClick={() => onSort("billingPeriod")} />
                  <Th label="Açık mı" active={sortKey === "isPublic"} dir={sortDir} onClick={() => onSort("isPublic")} />
                  <th className="w-40 text-right">
                    <Button variant="outline" size="sm" onClick={load}>Yenile</Button>
                  </th>
                </tr>
              </thead>

              <tbody>
                {/* Loading skeleton */}
                {loading &&
                  Array.from({ length: 6 }).map((_, i) => (
                    <tr key={`sk-${i}`} className="border-b animate-pulse">
                      <td className="py-2"><div className="h-4 w-20 rounded bg-muted" /></td>
                      <td><div className="h-4 w-40 rounded bg-muted" /></td>
                      <td><div className="h-4 w-16 rounded bg-muted" /></td>
                      <td><div className="h-4 w-14 rounded bg-muted" /></td>
                      <td><div className="h-4 w-16 rounded bg-muted" /></td>
                      <td><div className="h-5 w-14 rounded bg-muted" /></td>
                      <td className="text-right"><div className="h-7 w-20 rounded bg-muted ml-auto" /></td>
                    </tr>
                  ))}

                {/* Rows */}
                {!loading &&
                  items.map(p => (
                    <tr key={p.id} className="border-b hover:bg-muted/30">
                      <td className="py-2 align-middle">
                        <code className="rounded bg-muted px-1.5 py-0.5 text-xs">{p.code}</code>
                      </td>
                      <td className="align-middle">{p.name}</td>
                      <td className="align-middle">{fmtPrice(p.price, p.currency)}</td>
                      <td className="align-middle">
                        <Badge variant="outline">{CURRENCY[p.currency] ?? p.currency}</Badge>
                      </td>
                      <td className="align-middle">
                        <Badge>{PERIOD[p.billingPeriod] ?? p.billingPeriod}</Badge>
                      </td>
                      <td className="align-middle">
                        <Badge variant={p.isPublic ? "default" : "outline"} className={cn(!p.isPublic && "text-muted-foreground")}>
                          {p.isPublic ? "Evet" : "Hayır"}
                        </Badge>
                      </td>
                      <td className="text-right align-middle">
                        <Button size="sm" variant="outline" onClick={() => togglePublic(p.id, !p.isPublic)}>
                          {p.isPublic ? "Gizle" : "Aç"}
                        </Button>
                      </td>
                    </tr>
                  ))}

                {/* Empty */}
                {!loading && !items.length && (
                  <tr>
                    <td className="py-6 text-muted-foreground" colSpan={7}>Kayıt yok</td>
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
              items.map(p => (
                <div key={p.id} className="rounded-xl border p-3">
                  <div className="flex items-start justify-between gap-2">
                    <div>
                      <div className="text-sm font-medium">{p.name}</div>
                      <div className="text-xs text-muted-foreground mt-0.5">
                        Kod: <code className="bg-muted px-1 py-0.5 rounded">{p.code}</code>
                      </div>
                      <div className="mt-1 flex flex-wrap gap-2">
                        <Badge variant="outline">{CURRENCY[p.currency] ?? p.currency}</Badge>
                        <Badge>{PERIOD[p.billingPeriod] ?? p.billingPeriod}</Badge>
                        <Badge variant={p.isPublic ? "default" : "outline"}>
                          {p.isPublic ? "Açık" : "Gizli"}
                        </Badge>
                      </div>
                      <div className="text-sm mt-1">{fmtPrice(p.price, p.currency)}</div>
                    </div>
                    <div className="flex flex-col gap-2">
                      <Button size="sm" variant="outline" onClick={() => togglePublic(p.id, !p.isPublic)}>
                        {p.isPublic ? "Gizle" : "Aç"}
                      </Button>
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

/* ---- Sıralama başlık bileşeni ---- */
function Th({
  label,
  active,
  dir,
  onClick,
}: {
  label: string;
  active?: boolean;
  dir?: SortDir;
  onClick?: () => void;
}) {
  return (
    <th className="select-none">
      <button
        type="button"
        onClick={onClick}
        className={cn(
          "inline-flex items-center gap-1 py-2",
          "text-left hover:underline decoration-dotted underline-offset-4",
          active ? "text-foreground" : "text-muted-foreground"
        )}
      >
        {label}
        {active && <span className="text-xs">{dir === "asc" ? "▲" : "▼"}</span>}
      </button>
    </th>
  );
}
