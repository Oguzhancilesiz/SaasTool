// src/app/(protected)/invoices/page.tsx
"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import { api } from "@/lib/api";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Pagination } from "@/components/pagination";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { toCsv, downloadCsv } from "@/lib/csv";
import { fmt, money } from "@/lib/enums";
import { toast } from "sonner";
import { cn } from "@/lib/utils";
import { InvoiceForm } from "@/components/forms/invoice-form";

import type { Paged, InvoiceDto, OrgDto, CustomerDto } from "@/types/dto";

type SortKey = "number" | "organization" | "customer" | "total" | "due" | "paid";
type SortDir = "asc" | "desc";
type InvoiceStatus = "all" | "paid" | "unpaid" | "overdue";

export default function InvoicesPage() {
  const [page, setPage] = useState(1);
  const pageSize = 20;

  // Filtreler
  const [orgId, setOrgId] = useState("");
  const [customerId, setCustomerId] = useState("");
  const [status, setStatus] = useState<InvoiceStatus>("all");
  const [from, setFrom] = useState(""); // yyyy-MM-dd
  const [to, setTo] = useState("");

  // Liste
  const [data, setData] = useState<Paged<InvoiceDto> | null>(null);
  const [loading, setLoading] = useState(false);
  const [err, setErr] = useState<string | null>(null);

  // Ref. listeler
  const [orgs, setOrgs] = useState<OrgDto[]>([]);
  const [customers, setCustomers] = useState<CustomerDto[]>([]);
  const [loadingRefs, setLoadingRefs] = useState(false);

  const [sortKey, setSortKey] = useState<SortKey>("due");
  const [sortDir, setSortDir] = useState<SortDir>("desc");

  const abortRef = useRef<AbortController | null>(null);

  // Referansları çek
  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        setLoadingRefs(true);
        const [o, c] = await Promise.all([
          api.get<Paged<OrgDto>>("/organizations?page=1&pageSize=500"),
          api.get<Paged<CustomerDto>>("/customers?page=1&pageSize=500"),
        ]);
        if (!cancelled) {
          setOrgs(o.data.items ?? []);
          setCustomers(c.data.items ?? []);
        }
      } catch {
        // boşver
      } finally {
        if (!cancelled) setLoadingRefs(false);
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
      if (orgId) p.organizationId = orgId;
      if (customerId) p.customerId = customerId;
      if (from) p.from = new Date(from).toISOString();
      if (to) p.to = new Date(to).toISOString();
      // Bazı backend’ler status parametresini anlamaz; sorun değil.
      if (status !== "all") p.status = status;

      const { data } = await api.get<Paged<InvoiceDto>>(
        `/invoices?${new URLSearchParams(p)}`,
        { signal: controller.signal as AbortSignal }
      );
      setData(data);
    } catch (e: any) {
      if (e?.name !== "CanceledError" && e?.code !== "ERR_CANCELED") {
        setErr(e?.response?.data?.message || e?.message || "Faturalar yüklenemedi.");
        toast.error("Faturalar yüklenemedi");
      }
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load().catch(() => {});
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [page, orgId, customerId, status, from, to]);

  // Yardımcılar
  const short = (id?: string | null) =>
    !id ? "-" : id.length > 12 ? id.slice(0, 8) + "…" + id.slice(-4) : id;

  const nameOf = <T extends { id: string; name?: string }>(list: T[], id?: string | null) => {
    if (!id) return "-";
    const x = list.find(i => i.id === id);
    return x?.name || short(id);
  };

  const orgName = (id?: string | null) => nameOf(orgs, id);
  const customerName = (id?: string | null) => nameOf(customers, id);

  const computeStatus = (inv: InvoiceDto): InvoiceStatus => {
    const now = Date.now();
    const paidTs = inv.paidAt ? new Date(inv.paidAt).getTime() : null;
    const dueTs = inv.dueDate ? new Date(inv.dueDate).getTime() : null;
    if (paidTs) return "paid";
    if (dueTs && dueTs < now) return "overdue";
    return "unpaid";
  };

  const statusBadge = (st: InvoiceStatus) => {
    if (st === "paid") return <Badge className="bg-emerald-600/10 text-emerald-700">Ödendi</Badge>;
    if (st === "overdue") return <Badge className="bg-red-600/10 text-red-700">Gecikmiş</Badge>;
    return <Badge variant="outline">Bekliyor</Badge>;
  };

  // Sıralama + durum filtresini client’ta da uygula
  const items = useMemo(() => {
    let arr = [...(data?.items ?? [])];
    if (status !== "all") {
      arr = arr.filter(x => computeStatus(x) === status);
    }
    arr.sort((a, b) => {
      let A: string | number = 0;
      let B: string | number = 0;
      switch (sortKey) {
        case "number":
          A = (a.invoiceNumber || "").toLowerCase();
          B = (b.invoiceNumber || "").toLowerCase();
          break;
        case "organization":
          A = orgName(a.organizationId).toLowerCase();
          B = orgName(b.organizationId).toLowerCase();
          break;
        case "customer":
          A = customerName(a.customerId).toLowerCase();
          B = customerName(b.customerId).toLowerCase();
          break;
        case "total":
          A = a.grandTotal ?? 0;
          B = b.grandTotal ?? 0;
          break;
        case "due":
          A = a.dueDate ? new Date(a.dueDate).getTime() : 0;
          B = b.dueDate ? new Date(b.dueDate).getTime() : 0;
          break;
        case "paid":
          A = a.paidAt ? new Date(a.paidAt).getTime() : 0;
          B = b.paidAt ? new Date(b.paidAt).getTime() : 0;
          break;
      }
      if (A < B) return sortDir === "asc" ? -1 : 1;
      if (A > B) return sortDir === "asc" ? 1 : -1;
      return 0;
    });
    return arr;
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [data?.items, status, sortKey, sortDir, orgs, customers]);

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
    const rows = items.map(x => ({
      No: x.invoiceNumber,
      Organizasyon: orgName(x.organizationId),
      Müşteri: customerName(x.customerId),
      Tutar: money(x.grandTotal, x.currency),
      Vade: fmt(x.dueDate),
      Ödendi: fmt(x.paidAt),
      Durum:
        computeStatus(x) === "paid"
          ? "Ödendi"
          : computeStatus(x) === "overdue"
          ? "Gecikmiş"
          : "Bekliyor",
    }));
    if (!rows.length) return toast.info("Kayıt yok");
    downloadCsv("invoices.csv", toCsv(rows));
  };

  return (
    <div className="grid gap-6">
      {/* Create */}
      <Card>
        <CardHeader>
          <CardTitle>Yeni Fatura</CardTitle>
        </CardHeader>
        <CardContent>
          <InvoiceForm onCreated={load} />
        </CardContent>
      </Card>

      {/* List */}
      <Card>
        <CardHeader>
          <div className="flex flex-col gap-2 sm:flex-row sm:items-end sm:justify-between">
            <div>
              <CardTitle>Faturalar</CardTitle>
              {err && <p className="text-xs text-red-500 mt-1">{err}</p>}
            </div>

            {/* Filtreler */}
            <div className="mt-2 grid grid-cols-1 sm:grid-cols-5 gap-2">
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
                value={customerId}
                onChange={e => {
                  setPage(1);
                  setCustomerId(e.target.value);
                }}
                className="h-9 rounded-md border bg-background px-2 text-sm"
                aria-label="Müşteri"
              >
                <option value="">Tüm Müşteriler</option>
                {customers.map(c => (
                  <option key={c.id} value={c.id}>
                    {c.name}
                  </option>
                ))}
              </select>

              <select
                value={status}
                onChange={e => {
                  setPage(1);
                  setStatus(e.target.value as InvoiceStatus);
                }}
                className="h-9 rounded-md border bg-background px-2 text-sm"
                aria-label="Durum"
              >
                <option value="all">Tüm Durumlar</option>
                <option value="paid">Ödendi</option>
                <option value="unpaid">Bekliyor</option>
                <option value="overdue">Gecikmiş</option>
              </select>

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
                  <Th label="No" active={sortKey === "number"} dir={sortDir} onClick={() => onSort("number")} />
                  <Th label="Organizasyon" active={sortKey === "organization"} dir={sortDir} onClick={() => onSort("organization")} />
                  <Th label="Müşteri" active={sortKey === "customer"} dir={sortDir} onClick={() => onSort("customer")} />
                  <Th label="Tutar" active={sortKey === "total"} dir={sortDir} onClick={() => onSort("total")} />
                  <Th label="Vade" active={sortKey === "due"} dir={sortDir} onClick={() => onSort("due")} />
                  <Th label="Ödendi" active={sortKey === "paid"} dir={sortDir} onClick={() => onSort("paid")} />
                  <th className="w-24 text-right">Durum</th>
                </tr>
              </thead>

              <tbody>
                {/* Skeleton */}
                {loading &&
                  Array.from({ length: 8 }).map((_, i) => (
                    <tr key={`sk-${i}`} className="border-b animate-pulse">
                      <td className="py-2"><div className="h-4 w-20 rounded bg-muted" /></td>
                      <td><div className="h-4 w-40 rounded bg-muted" /></td>
                      <td><div className="h-4 w-44 rounded bg-muted" /></td>
                      <td><div className="h-4 w-28 rounded bg-muted" /></td>
                      <td><div className="h-4 w-28 rounded bg-muted" /></td>
                      <td><div className="h-4 w-28 rounded bg-muted" /></td>
                      <td className="text-right"><div className="h-6 w-14 rounded bg-muted ml-auto" /></td>
                    </tr>
                  ))}

                {/* Satırlar */}
                {!loading &&
                  items.map(x => {
                    const st = computeStatus(x);
                    return (
                      <tr key={x.id} className="border-b hover:bg-muted/30">
                        <td className="py-2">{x.invoiceNumber}</td>
                        <td>{orgName(x.organizationId)}</td>
                        <td>{customerName(x.customerId)}</td>
                        <td className="font-medium">{money(x.grandTotal, x.currency)}</td>
                        <td>{fmt(x.dueDate)}</td>
                        <td>{fmt(x.paidAt)}</td>
                        <td className="text-right">{statusBadge(st)}</td>
                      </tr>
                    );
                  })}

                {/* Boş durum */}
                {!loading && !(items?.length ?? 0) && (
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
              items.map(x => {
                const st = computeStatus(x);
                return (
                  <div key={x.id} className="rounded-xl border p-3">
                    <div className="flex items-start justify-between gap-2">
                      <div className="space-y-1">
                        <div className="text-sm font-medium">{x.invoiceNumber}</div>
                        <div className="text-xs text-muted-foreground">
                          {orgName(x.organizationId)} · {customerName(x.customerId)}
                        </div>
                        <div className="text-xs">
                          {fmt(x.dueDate)} · {fmt(x.paidAt) || "—"}
                        </div>
                        <div className="text-sm font-medium">{money(x.grandTotal, x.currency)}</div>
                      </div>
                      {statusBadge(st)}
                    </div>
                  </div>
                );
              })}
            {!loading && !(items?.length ?? 0) && (
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
