// src/app/(protected)/subscriptions/page.tsx
"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import { api } from "@/lib/api";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Pagination } from "@/components/pagination";
import { SubscriptionForm } from "@/components/forms/subscription-form";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { toCsv, downloadCsv } from "@/lib/csv";
import { fmt } from "@/lib/enums";
import { toast } from "sonner";

import type { Paged } from "@/types/dto";
import type {
  SubscriptionDto,
  OrgDto,
  AppDto,
  PlanDto,
  CustomerDto,
} from "@/types/dto";

// Not: Backend arayüzlerinde farklı adlar varsa kendi tiplerinle eşleştir.
type SortKey = "organization" | "app" | "plan" | "customer" | "startsAt" | "endsAt";
type SortDir = "asc" | "desc";

export default function SubscriptionsPage() {
  const [page, setPage] = useState(1);
  const pageSize = 20;

  // Filtreler
  const [orgId, setOrgId] = useState("");
  const [appId, setAppId] = useState("");
  const [planId, setPlanId] = useState("");

  const [data, setData] = useState<Paged<SubscriptionDto> | null>(null);
  const [loading, setLoading] = useState(false);
  const [err, setErr] = useState<string | null>(null);

  // Referans listeler (id->ad)
  const [orgs, setOrgs] = useState<OrgDto[]>([]);
  const [apps, setApps] = useState<AppDto[]>([]);
  const [plans, setPlans] = useState<PlanDto[]>([]);
  const [customers, setCustomers] = useState<CustomerDto[]>([]);

  // Sıralama
  const [sortKey, setSortKey] = useState<SortKey>("startsAt");
  const [sortDir, setSortDir] = useState<SortDir>("desc");

  const abortRef = useRef<AbortController | null>(null);

  // Ref. listeleri tek sefer çek
  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const [o, a, p] = await Promise.all([
          api.get<Paged<OrgDto>>("/organizations?page=1&pageSize=500"),
          api.get<Paged<AppDto>>("/apps?page=1&pageSize=500"),
          api.get<Paged<PlanDto>>("/plans?page=1&pageSize=500"),
        ]);
        if (!cancelled) {
          setOrgs(o.data.items ?? []);
          setApps(a.data.items ?? []);
          setPlans(p.data.items ?? []);
        }
      } catch {
        // Referanslar gelmezse de isim yerine kibar id kısaltması gösteririz.
      }
      // Müşterileri çok şişirmeden ilk 500 çekelim
      try {
        const c = await api.get<Paged<CustomerDto>>("/customers?page=1&pageSize=500");
        if (!cancelled) setCustomers(c.data.items ?? []);
      } catch {
        // boş geç
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
      if (orgId) p.organizationId = orgId;
      if (appId) p.appId = appId;
      if (planId) p.planId = planId;

      const res = await api.get<Paged<SubscriptionDto>>(
        `/subscriptions?${new URLSearchParams(p)}`,
        { signal: controller.signal as AbortSignal }
      );
      setData(res.data);
    } catch (e: any) {
      if (e?.name !== "CanceledError" && e?.code !== "ERR_CANCELED") {
        setErr(e?.response?.data?.message || e?.message || "Liste yüklenemedi.");
        toast.error("Abonelikler yüklenemedi");
      }
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load().catch(() => {});
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [page, orgId, appId, planId]);

  // id -> ad yardımcıları
  const short = (id?: string | null) =>
    !id ? "-" : id.length > 12 ? id.slice(0, 8) + "…" + id.slice(-4) : id;

  const nameOf = <T extends { id: string; name?: string; code?: string }>(
    list: T[],
    id?: string | null
  ) => {
    if (!id) return "-";
    const m = list.find(x => x.id === id);
    return m?.name || m?.code || short(id);
  };

  const orgName = (id?: string | null) => nameOf(orgs, id);
  const appName = (id?: string | null) => nameOf(apps, id);
  const planName = (id?: string | null) => nameOf(plans, id);
  const customerName = (id?: string | null) => {
    const c = customers.find(x => x.id === id);
    return c?.name || short(id);
  };

  // Durum rozeti (elde varsa)
  const statusBadge = (s: SubscriptionDto) => {
    const state = (s as any).subscriptionState as number | undefined;
    const trialEndsAt = (s as any).trialEndsAt as string | null | undefined;
    const endsAt = s.endsAt ? new Date(s.endsAt).getTime() : null;
    const now = Date.now();

    if (state !== undefined) {
      // Backend enum’unun sırası sende neyse map edersin; ben kaba yorumlayacağım
      // 0 Unknown / 1 Trial / 2 Active / 3 Paused / 4 Canceled vs. varsayım
      if (state === 1) return <Badge variant="outline">Trial</Badge>;
      if (state === 2) return <Badge className="bg-emerald-600/10 text-emerald-600">Aktif</Badge>;
      if (state === 4 || (endsAt && endsAt < now))
        return <Badge variant="outline">Bitti</Badge>;
      return <Badge variant="outline">Durum</Badge>;
    }
    // Enum yoksa tarihlerle kaba çıkarım
    if (endsAt && endsAt < now) return <Badge variant="outline">Bitti</Badge>;
    if (trialEndsAt && new Date(trialEndsAt).getTime() > now)
      return <Badge variant="outline">Trial</Badge>;
    return <Badge className="bg-emerald-600/10 text-emerald-600">Aktif</Badge>;
  };

  // Sıralama
  const items = useMemo(() => {
    const arr = [...(data?.items ?? [])];
    arr.sort((a, b) => {
      let A: string | number = "";
      let B: string | number = "";
      switch (sortKey) {
        case "organization":
          A = orgName(a.organizationId).toLowerCase();
          B = orgName(b.organizationId).toLowerCase();
          break;
        case "app":
          A = appName(a.appId).toLowerCase();
          B = appName(b.appId).toLowerCase();
          break;
        case "plan":
          A = planName(a.planId).toLowerCase();
          B = planName(b.planId).toLowerCase();
          break;
        case "customer":
          A = customerName(a.customerId).toLowerCase();
          B = customerName(b.customerId).toLowerCase();
          break;
        case "startsAt":
          A = new Date(a.startsAt).getTime();
          B = new Date(b.startsAt).getTime();
          break;
        case "endsAt":
          A = a.endsAt ? new Date(a.endsAt).getTime() : 0;
          B = b.endsAt ? new Date(b.endsAt).getTime() : 0;
          break;
      }
      if (A < B) return sortDir === "asc" ? -1 : 1;
      if (A > B) return sortDir === "asc" ? 1 : -1;
      return 0;
    });
    return arr;
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [data?.items, sortKey, sortDir, orgs, apps, plans, customers]);

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
    const rows = (data?.items ?? []).map(s => ({
      Organizasyon: orgName(s.organizationId),
      Uygulama: appName(s.appId),
      Plan: planName(s.planId),
      Müşteri: customerName(s.customerId),
      Başlangıç: fmt(s.startsAt),
      Bitiş: fmt(s.endsAt),
    }));
    if (!rows.length) return toast.info("Kayıt yok");
    downloadCsv("subscriptions.csv", toCsv(rows));
  };

  return (
    <div className="grid gap-6">
      {/* Create */}
      <Card>
        <CardHeader>
          <CardTitle>Yeni Abonelik</CardTitle>
        </CardHeader>
        <CardContent>
          <SubscriptionForm onCreated={load} />
        </CardContent>
      </Card>

      {/* List */}
      <Card>
        <CardHeader>
          <div className="flex flex-col gap-2 sm:flex-row sm:items-end sm:justify-between">
            <div>
              <CardTitle>Abonelikler</CardTitle>
              {err && <p className="text-xs text-red-500 mt-1">{err}</p>}
            </div>

            {/* Filtre & Aksiyonlar */}
            <div className="mt-3 sm:mt-0 grid grid-cols-1 sm:grid-cols-4 gap-2">
              <select
                value={orgId}
                onChange={e => {
                  setPage(1);
                  setOrgId(e.target.value);
                }}
                className="h-9 rounded-md border bg-background px-2 text-sm"
                aria-label="Organizasyon"
              >
                <option value="">Tüm Organizasyonlar</option>
                {orgs.map(o => (
                  <option key={o.id} value={o.id}>
                    {o.name}
                  </option>
                ))}
              </select>

              <select
                value={appId}
                onChange={e => {
                  setPage(1);
                  setAppId(e.target.value);
                }}
                className="h-9 rounded-md border bg-background px-2 text-sm"
                aria-label="Uygulama"
              >
                <option value="">Tüm Uygulamalar</option>
                {apps.map(a => (
                  <option key={a.id} value={a.id}>
                    {a.name}
                  </option>
                ))}
              </select>

              <select
                value={planId}
                onChange={e => {
                  setPage(1);
                  setPlanId(e.target.value);
                }}
                className="h-9 rounded-md border bg-background px-2 text-sm"
                aria-label="Plan"
              >
                <option value="">Tüm Planlar</option>
                {plans.map(p => (
                  <option key={p.id} value={p.id}>
                    {p.name ?? p.code}
                  </option>
                ))}
              </select>

              <div className="flex items-center justify-end">
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
                  <Th label="Org" active={sortKey === "organization"} dir={sortDir} onClick={() => onSort("organization")} />
                  <Th label="App" active={sortKey === "app"} dir={sortDir} onClick={() => onSort("app")} />
                  <Th label="Plan" active={sortKey === "plan"} dir={sortDir} onClick={() => onSort("plan")} />
                  <Th label="Müşteri" active={sortKey === "customer"} dir={sortDir} onClick={() => onSort("customer")} />
                  <Th label="Başlangıç" active={sortKey === "startsAt"} dir={sortDir} onClick={() => onSort("startsAt")} />
                  <Th label="Bitiş" active={sortKey === "endsAt"} dir={sortDir} onClick={() => onSort("endsAt")} />
                  <th className="w-20 text-right">Durum</th>
                </tr>
              </thead>
              <tbody>
                {/* Loading skeleton */}
                {loading &&
                  Array.from({ length: 6 }).map((_, i) => (
                    <tr key={`sk-${i}`} className="border-b animate-pulse">
                      <td className="py-2"><div className="h-4 w-40 rounded bg-muted" /></td>
                      <td><div className="h-4 w-36 rounded bg-muted" /></td>
                      <td><div className="h-4 w-32 rounded bg-muted" /></td>
                      <td><div className="h-4 w-44 rounded bg-muted" /></td>
                      <td><div className="h-4 w-28 rounded bg-muted" /></td>
                      <td><div className="h-4 w-28 rounded bg-muted" /></td>
                      <td className="text-right"><div className="h-6 w-12 rounded bg-muted ml-auto" /></td>
                    </tr>
                  ))}

                {/* Satırlar */}
                {!loading &&
                  items.map(s => (
                    <tr key={s.id} className="border-b hover:bg-muted/30">
                      <td className="py-2">{orgName(s.organizationId)}</td>
                      <td>{appName(s.appId)}</td>
                      <td>{planName(s.planId)}</td>
                      <td>{customerName(s.customerId)}</td>
                      <td>{fmt(s.startsAt)}</td>
                      <td>{fmt(s.endsAt)}</td>
                      <td className="text-right">{statusBadge(s)}</td>
                    </tr>
                  ))}

                {/* Boş durum */}
                {!loading && !(data?.items?.length ?? 0) && (
                  <tr>
                    <td colSpan={7} className="py-6 text-center text-muted-foreground">
                      Kayıt yok
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>

          {/* Mobil kartlar */}
          <div className="mt-4 grid gap-3 sm:hidden">
            {loading &&
              Array.from({ length: 4 }).map((_, i) => (
                <div key={`skm-${i}`} className="h-24 rounded-xl border animate-pulse bg-muted/40" />
              ))}
            {!loading &&
              items.map(s => (
                <div key={s.id} className="rounded-xl border p-3">
                  <div className="flex items-start justify-between gap-2">
                    <div className="space-y-1">
                      <div className="text-sm font-medium">{planName(s.planId)}</div>
                      <div className="text-xs text-muted-foreground">
                        {orgName(s.organizationId)} · {appName(s.appId)}
                      </div>
                      <div className="text-xs">Müşteri: {customerName(s.customerId)}</div>
                      <div className="text-xs">
                        {fmt(s.startsAt)} — {fmt(s.endsAt)}
                      </div>
                    </div>
                    {statusBadge(s)}
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

/* ---- Tabloda tıklanabilir başlık ---- */
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
        className={[
          "inline-flex items-center gap-1 py-2",
          "text-left hover:underline decoration-dotted underline-offset-4",
          active ? "text-foreground" : "text-muted-foreground",
        ].join(" ")}
      >
        {label}
        {active && <span className="text-xs">{dir === "asc" ? "▲" : "▼"}</span>}
      </button>
    </th>
  );
}
