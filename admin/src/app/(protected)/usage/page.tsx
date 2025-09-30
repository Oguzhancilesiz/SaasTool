// src/app/(protected)/usage/page.tsx
"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import { api } from "@/lib/api";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Pagination } from "@/components/pagination";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { toCsv, downloadCsv } from "@/lib/csv";
import { fmt, PeriodUnitMap } from "@/lib/enums";
import { toast } from "sonner";
import { cn } from "@/lib/utils";

type Paged<T> = { items: T[]; page: number; pageSize: number; total: number };

// Kendi DTO’larına uyacak kadar gevşek tipler
type UsageRecordDto = {
  id: string;
  subscriptionId: string;
  featureId: string;
  periodUnit: number;       // 0=Gün, 1=Hafta, vs. senin map’ine göre
  periodStart: string;
  periodEnd: string;
  usedValue: number;
};

type IdName = { id: string; name?: string; code?: string };
type SubscriptionLite = {
  id: string;
  organizationId?: string | null;
  appId?: string | null;
  planId?: string | null;
  customerId?: string | null;
};

type SortKey = "subscription" | "feature" | "periodStart" | "periodEnd" | "usedValue";
type SortDir = "asc" | "desc";

export default function UsagePage() {
  const [page, setPage] = useState(1);
  const pageSize = 50;

  // Filtreler
  const [subscriptionId, setSubscriptionId] = useState("");
  const [featureId, setFeatureId] = useState("");
  const [periodUnit, setPeriodUnit] = useState<string>(""); // ""=hepsi
  const [from, setFrom] = useState<string>(""); // yyyy-MM-dd
  const [to, setTo] = useState<string>("");

  // Liste
  const [data, setData] = useState<Paged<UsageRecordDto> | null>(null);
  const [loading, setLoading] = useState(false);
  const [err, setErr] = useState<string | null>(null);
  const abortRef = useRef<AbortController | null>(null);

  // Referans listeler (isim çözmek için)
  const [subs, setSubs] = useState<SubscriptionLite[]>([]);
  const [features, setFeatures] = useState<IdName[]>([]);
  const [orgs, setOrgs] = useState<IdName[]>([]);
  const [apps, setApps] = useState<IdName[]>([]);
  const [plans, setPlans] = useState<IdName[]>([]);
  const [customers, setCustomers] = useState<IdName[]>([]);

  const [sortKey, setSortKey] = useState<SortKey>("periodStart");
  const [sortDir, setSortDir] = useState<SortDir>("desc");

  // Referansları çek (tek sefer)
  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const [s, f, o, a, p, c] = await Promise.all([
          api.get<Paged<SubscriptionLite>>("/subscriptions?page=1&pageSize=500"),
          api.get<Paged<IdName>>("/features?page=1&pageSize=500"),
          api.get<Paged<IdName>>("/organizations?page=1&pageSize=500"),
          api.get<Paged<IdName>>("/apps?page=1&pageSize=500"),
          api.get<Paged<IdName>>("/plans?page=1&pageSize=500"),
          api.get<Paged<IdName>>("/customers?page=1&pageSize=500"),
        ]);
        if (!cancelled) {
          setSubs(s.data.items ?? []);
          setFeatures(f.data.items ?? []);
          setOrgs(o.data.items ?? []);
          setApps(a.data.items ?? []);
          setPlans(p.data.items ?? []);
          setCustomers(c.data.items ?? []);
        }
      } catch {
        // sessiz
      }
    })();
    return () => {
      cancelled = true;
    };
  }, []);

  // Listeyi yükle
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
      if (subscriptionId) p.subscriptionId = subscriptionId;
      if (featureId) p.featureId = featureId;
      if (periodUnit) p.periodUnit = periodUnit;
      if (from) p.from = new Date(from).toISOString();
      if (to) p.to = new Date(to).toISOString();

      const { data } = await api.get<Paged<UsageRecordDto>>(
        `/usage?${new URLSearchParams(p)}`,
        { signal: controller.signal as AbortSignal }
      );
      setData(data);
    } catch (e: any) {
      if (e?.name !== "CanceledError" && e?.code !== "ERR_CANCELED") {
        setErr(e?.response?.data?.message || e?.message || "Kayıtlar yüklenemedi.");
        toast.error("Kullanım kayıtları yüklenemedi");
      }
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load().catch(() => {});
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [page, subscriptionId, featureId, periodUnit, from, to]);

  // İsim çözme yardımcıları
  const short = (id?: string | null) =>
    !id ? "-" : id.length > 12 ? id.slice(0, 8) + "…" + id.slice(-4) : id;

  const nameOf = (list: IdName[], id?: string | null) => {
    if (!id) return "-";
    const m = list.find(x => x.id === id);
    return m?.name || m?.code || short(id);
  };

  const subLabel = (id: string) => {
    const s = subs.find(x => x.id === id);
    if (!s) return short(id);
    const parts = [
      nameOf(orgs, s.organizationId),
      nameOf(apps, s.appId),
      nameOf(plans, s.planId),
      nameOf(customers, s.customerId),
    ].filter(Boolean);
    return parts.join(" · ");
  };

  const featureLabel = (id: string) => nameOf(features, id);

  // Sıralama
  const items = useMemo(() => {
    const arr = [...(data?.items ?? [])];
    arr.sort((a, b) => {
      let A: string | number = "";
      let B: string | number = "";
      switch (sortKey) {
        case "subscription":
          A = subLabel(a.subscriptionId).toLowerCase();
          B = subLabel(b.subscriptionId).toLowerCase();
          break;
        case "feature":
          A = featureLabel(a.featureId).toLowerCase();
          B = featureLabel(b.featureId).toLowerCase();
          break;
        case "periodStart":
          A = new Date(a.periodStart).getTime();
          B = new Date(b.periodStart).getTime();
          break;
        case "periodEnd":
          A = new Date(a.periodEnd).getTime();
          B = new Date(b.periodEnd).getTime();
          break;
        case "usedValue":
          A = a.usedValue;
          B = b.usedValue;
          break;
      }
      if (A < B) return sortDir === "asc" ? -1 : 1;
      if (A > B) return sortDir === "asc" ? 1 : -1;
      return 0;
    });
    return arr;
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [data?.items, sortKey, sortDir, subs, features, orgs, apps, plans, customers]);

  const onSort = (key: SortKey) => {
    setSortKey(k => {
      if (k !== key) {
        setSortDir("asc");
        return key;
      }
      setSortDir(d => (d === "asc" ? "desc" : "asc"));
      return key;
    });
  };

  // CSV
  const exportCsv = () => {
    const rows = (data?.items ?? []).map(x => ({
      Abonelik: subLabel(x.subscriptionId),
      Özellik: featureLabel(x.featureId),
      Birim: PeriodUnitMap[x.periodUnit] ?? x.periodUnit,
      Başlangıç: fmt(x.periodStart),
      Bitiş: fmt(x.periodEnd),
      Değer: x.usedValue,
    }));
    if (!rows.length) return toast.info("Kayıt yok");
    downloadCsv("usage.csv", toCsv(rows));
  };

  // Period seçenekleri (senin map’in neyse onu saydır)
  const periodOptions = useMemo(() => {
    const seen = new Set<number>();
    (data?.items ?? []).forEach(x => seen.add(x.periodUnit));
    // Tümünü de göstermek için sabit liste istiyorsan elle yaz: [0,1,2,3,4,5]
    return Array.from(seen.values()).sort((a, b) => a - b);
  }, [data?.items]);

  return (
    <div className="grid gap-6">
      <Card>
        <CardHeader>
          <div className="flex flex-col gap-2 sm:flex-row sm:items-end sm:justify-between">
            <div>
              <CardTitle>Kullanım Kayıtları</CardTitle>
              {err && <p className="text-xs text-red-500 mt-1">{err}</p>}
            </div>

            {/* Filtreler */}
            <div className="mt-2 grid grid-cols-1 sm:grid-cols-5 gap-2">
              {/* Abonelik */}
              <select
                value={subscriptionId}
                onChange={e => {
                  setPage(1);
                  setSubscriptionId(e.target.value);
                }}
                className="h-9 rounded-md border bg-background px-2 text-sm"
                aria-label="Abonelik"
              >
                <option value="">Tüm Abonelikler</option>
                {subs.map(s => (
                  <option key={s.id} value={s.id}>
                    {subLabel(s.id)}
                  </option>
                ))}
              </select>

              {/* Özellik */}
              <select
                value={featureId}
                onChange={e => {
                  setPage(1);
                  setFeatureId(e.target.value);
                }}
                className="h-9 rounded-md border bg-background px-2 text-sm"
                aria-label="Özellik"
              >
                <option value="">Tüm Özellikler</option>
                {features.map(f => (
                  <option key={f.id} value={f.id}>
                    {f.name || f.code || short(f.id)}
                  </option>
                ))}
              </select>

              {/* Period */}
              <select
                value={periodUnit}
                onChange={e => {
                  setPage(1);
                  setPeriodUnit(e.target.value);
                }}
                className="h-9 rounded-md border bg-background px-2 text-sm"
                aria-label="Periyot"
              >
                <option value="">Tüm Periyotlar</option>
                {periodOptions.map(u => (
                  <option key={u} value={String(u)}>
                    {PeriodUnitMap[u] ?? u}
                  </option>
                ))}
              </select>

              {/* Tarih aralığı */}
              <input
                type="date"
                value={from}
                onChange={e => {
                  setPage(1);
                  setFrom(e.target.value);
                }}
                className="h-9 rounded-md border bg-background px-2 text-sm"
                aria-label="Başlangıç"
              />
              <div className="flex items-center gap-2">
                <input
                  type="date"
                  value={to}
                  onChange={e => {
                    setPage(1);
                    setTo(e.target.value);
                  }}
                  className="h-9 rounded-md border bg-background px-2 text-sm flex-1"
                  aria-label="Bitiş"
                />
                <Button variant="outline" size="sm" onClick={exportCsv}>
                  CSV
                </Button>
              </div>
            </div>
          </div>
        </CardHeader>

        <CardContent>
          {/* Desktop tablo */}
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="sticky top-0 z-10 bg-background">
                <tr className="border-b text-left">
                  <Th label="Abonelik" active={sortKey === "subscription"} dir={sortDir} onClick={() => onSort("subscription")} />
                  <Th label="Özellik" active={sortKey === "feature"} dir={sortDir} onClick={() => onSort("feature")} />
                  <Th label="Birim" />
                  <Th label="Başlangıç" active={sortKey === "periodStart"} dir={sortDir} onClick={() => onSort("periodStart")} />
                  <Th label="Bitiş" active={sortKey === "periodEnd"} dir={sortDir} onClick={() => onSort("periodEnd")} />
                  <Th label="Değer" active={sortKey === "usedValue"} dir={sortDir} onClick={() => onSort("usedValue")} />
                </tr>
              </thead>

              <tbody>
                {/* Skeleton */}
                {loading &&
                  Array.from({ length: 8 }).map((_, i) => (
                    <tr key={`sk-${i}`} className="border-b animate-pulse">
                      <td className="py-2"><div className="h-4 w-56 rounded bg-muted" /></td>
                      <td><div className="h-4 w-40 rounded bg-muted" /></td>
                      <td><div className="h-4 w-24 rounded bg-muted" /></td>
                      <td><div className="h-4 w-28 rounded bg-muted" /></td>
                      <td><div className="h-4 w-28 rounded bg-muted" /></td>
                      <td><div className="h-4 w-16 rounded bg-muted" /></td>
                    </tr>
                  ))}

                {/* Satırlar */}
                {!loading &&
                  items.map(x => (
                    <tr key={x.id} className="border-b hover:bg-muted/30">
                      <td className="py-2">{subLabel(x.subscriptionId)}</td>
                      <td>{featureLabel(x.featureId)}</td>
                      <td>
                        <Badge variant="outline">{PeriodUnitMap[x.periodUnit] ?? x.periodUnit}</Badge>
                      </td>
                      <td>{fmt(x.periodStart)}</td>
                      <td>{fmt(x.periodEnd)}</td>
                      <td className="font-medium">{x.usedValue}</td>
                    </tr>
                  ))}

                {/* Boş durum */}
                {!loading && !(data?.items?.length ?? 0) && (
                  <tr>
                    <td colSpan={6} className="py-6 text-center text-muted-foreground">
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
                      <div className="text-sm font-medium">{featureLabel(x.featureId)}</div>
                      <div className="text-xs text-muted-foreground">
                        {subLabel(x.subscriptionId)}
                      </div>
                      <div className="text-xs mt-1">
                        {fmt(x.periodStart)} — {fmt(x.periodEnd)} ·{" "}
                        <span className="font-medium">{x.usedValue}</span>
                      </div>
                    </div>
                    <Badge variant="outline">{PeriodUnitMap[x.periodUnit] ?? x.periodUnit}</Badge>
                  </div>
                </div>
              ))}

            {!loading && !(data?.items?.length ?? 0) && (
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

/* ---- Tıklanabilir başlık ---- */
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
