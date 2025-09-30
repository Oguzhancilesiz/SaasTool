// src/app/(protected)/customers/page.tsx
"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import { api } from "@/lib/api";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { ListToolbar } from "@/components/list-toolbar";
import { Pagination } from "@/components/pagination";
import { ConfirmButton } from "@/components/confirm-button";
import { toCsv, downloadCsv } from "@/lib/csv";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";
import { CustomerForm } from "@/components/forms/customer-form";

type SortKey = "name" | "email" | "organization";
type SortDir = "asc" | "desc";

export default function CustomersPage() {
  const [page, setPage] = useState(1);
  const pageSize = 20;

  // Arama -> debounce -> q
  const [searchTerm, setSearchTerm] = useState("");
  const [q, setQ] = useState("");

  // Org filtresi
  const [orgId, setOrgId] = useState("");

  const [data, setData] = useState<Paged<CustomerDto> | null>(null);
  const [orgs, setOrgs] = useState<OrgDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [err, setErr] = useState<string | null>(null);

  const [sortKey, setSortKey] = useState<SortKey>("name");
  const [sortDir, setSortDir] = useState<SortDir>("asc");

  const [selected, setSelected] = useState<Set<string>>(new Set());
  const abortRef = useRef<AbortController | null>(null);

  // Debounce
  useEffect(() => {
    const t = setTimeout(() => {
      setPage(1);
      setQ(searchTerm.trim());
    }, 350);
    return () => clearTimeout(t);
  }, [searchTerm]);

  // Org listesi (id -> ad)
  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const res = await api.get<Paged<OrgDto>>("/organizations?page=1&pageSize=500");
        if (!cancelled) setOrgs(res.data.items ?? []);
      } catch {
        // isim bulunamazsa id kısaltması kullanacağız
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
      const p: Record<string, string> = { page: String(page), pageSize: String(pageSize) };
      if (q) p.search = q;
      if (orgId) p.organizationId = orgId;
      const { data } = await api.get<Paged<CustomerDto>>(`/customers?${new URLSearchParams(p)}`, {
        signal: controller.signal as AbortSignal,
      });
      setData(data);
      setSelected(new Set());
    } catch (e: any) {
      if (e?.name !== "CanceledError" && e?.code !== "ERR_CANCELED") {
        setErr(e?.response?.data?.message || e?.message || "Liste yüklenemedi.");
        toast.error("Müşteriler yüklenemedi");
      }
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load().catch(() => {});
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [page, q, orgId]);

  const del = async (id: string) => {
    await api.delete(`/customers/${id}`);
    toast.success("Silindi");
    await load();
  };

  const delMany = async (ids: string[]) => {
    if (!ids.length) return;
    try {
      await api.delete(`/customers`, { data: { ids } as any });
      toast.success(`${ids.length} müşteri silindi`);
      await load();
    } catch {
      toast.error("Toplu silme başarısız");
    }
  };

  // id -> ad
  const orgName = (id?: string | null) => {
    if (!id) return "-";
    const o = orgs.find(x => x.id === id);
    if (o?.name) return o.name;
    return id.length > 12 ? id.slice(0, 8) + "…" + id.slice(-4) : id;
  };

  // CSV: seçili varsa seçili, yoksa sayfadaki tüm satırlar
  const exportCsv = () => {
    const src = (data?.items ?? []).filter(x => !selected.size || selected.has(x.id));
    const rows = src.map(x => ({
      Ad: x.name,
      Email: x.email,
      Organizasyon: orgName(x.organizationId),
    }));
    if (!rows.length) return toast.info("Kayıt yok");
    downloadCsv("customers.csv", toCsv(rows));
  };

  // Sayfa içi sıralama
  const items = useMemo(() => {
    const arr = [...(data?.items ?? [])];
    arr.sort((a, b) => {
      const A =
        sortKey === "organization"
          ? orgName(a.organizationId).toLowerCase()
          : String((a as any)[sortKey] ?? "").toLowerCase();
      const B =
        sortKey === "organization"
          ? orgName(b.organizationId).toLowerCase()
          : String((b as any)[sortKey] ?? "").toLowerCase();
      if (A < B) return sortDir === "asc" ? -1 : 1;
      if (A > B) return sortDir === "asc" ? 1 : -1;
      return 0;
    });
    return arr;
  }, [data?.items, sortKey, sortDir, orgs]);

  // seçim
  const allChecked = items.length > 0 && items.every(x => selected.has(x.id));
  const toggleAll = () => {
    if (allChecked) setSelected(new Set());
    else setSelected(new Set(items.map(x => x.id)));
  };
  const toggleOne = (id: string) => {
    setSelected(prev => {
      const next = new Set(prev);
      next.has(id) ? next.delete(id) : next.add(id);
      return next;
    });
  };

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

  return (
    <div className="grid gap-6">
      {/* Create */}
      <Card>
        <CardHeader>
          <CardTitle>Yeni Müşteri</CardTitle>
        </CardHeader>
        <CardContent>
          <CustomerForm onCreated={load} />
        </CardContent>
      </Card>

      {/* List */}
      <Card>
        <CardHeader>
          <div className="flex flex-col gap-2 sm:flex-row sm:items-end sm:justify-between">
            <div>
              <CardTitle>Müşteriler</CardTitle>
              {err && <p className="text-xs text-red-500 mt-1">{err}</p>}
            </div>

            <div className="mt-3 sm:mt-0 flex flex-col sm:flex-row gap-2 sm:items-center">
              {/* Org filtresi */}
              <div className="flex items-center gap-2">
                <label htmlFor="org-filter" className="text-xs text-muted-foreground whitespace-nowrap">
                  Organizasyon
                </label>
                <select
                  id="org-filter"
                  value={orgId}
                  onChange={e => {
                    setPage(1);
                    setOrgId(e.target.value);
                  }}
                  className="h-9 rounded-md border bg-background px-2 text-sm"
                >
                  <option value="">Tümü</option>
                  {orgs.map(o => (
                    <option key={o.id} value={o.id}>
                      {o.name}
                    </option>
                  ))}
                </select>
              </div>

              <ListToolbar onSearch={setSearchTerm} onRefresh={load} onExport={exportCsv} />
            </div>
          </div>
        </CardHeader>

        <CardContent>
          {/* Desktop tablo */}
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="sticky top-0 z-10 bg-background">
                <tr className="border-b text-left">
                  <th className="py-2 w-10">
                    <input
                      type="checkbox"
                      className="h-4 w-4 accent-primary"
                      checked={allChecked}
                      onChange={toggleAll}
                      aria-label="Tümünü seç"
                    />
                  </th>
                  <Th label="Ad" active={sortKey === "name"} dir={sortDir} onClick={() => onSort("name")} />
                  <Th label="Email" active={sortKey === "email"} dir={sortDir} onClick={() => onSort("email")} />
                  <Th label="Organizasyon" active={sortKey === "organization"} dir={sortDir} onClick={() => onSort("organization")} />
                  <th className="w-52 text-right">
                    <div className="flex items-center justify-end gap-2">
                      <Button variant="outline" size="sm" onClick={load}>Yenile</Button>
                      <ConfirmButton disabled={!selected.size} onConfirm={() => delMany([...selected])}>
                        Seçileni Sil
                      </ConfirmButton>
                    </div>
                  </th>
                </tr>
              </thead>

              <tbody>
                {/* Loading skeleton */}
                {loading &&
                  Array.from({ length: 6 }).map((_, i) => (
                    <tr key={`sk-${i}`} className="border-b animate-pulse">
                      <td className="py-2"><div className="h-4 w-4 rounded bg-muted" /></td>
                      <td><div className="h-4 w-32 rounded bg-muted" /></td>
                      <td><div className="h-4 w-40 rounded bg-muted" /></td>
                      <td><div className="h-4 w-36 rounded bg-muted" /></td>
                      <td className="text-right"><div className="h-7 w-24 rounded bg-muted ml-auto" /></td>
                    </tr>
                  ))}

                {/* Rows */}
                {!loading &&
                  items.map(x => (
                    <tr key={x.id} className="border-b hover:bg-muted/30">
                      <td className="py-2 align-middle">
                        <input
                          type="checkbox"
                          className="h-4 w-4 accent-primary"
                          checked={selected.has(x.id)}
                          onChange={() => toggleOne(x.id)}
                          aria-label={`${x.name} seç`}
                        />
                      </td>

                      <td className="py-2 align-middle">{x.name}</td>

                      <td className="align-middle">
                        <div className="inline-flex items-center gap-2">
                          <span>{x.email}</span>
                          <button
                            type="button"
                            className="text-xs text-muted-foreground hover:underline"
                            onClick={() => {
                              navigator.clipboard?.writeText(x.email);
                              toast.success("E-posta kopyalandı");
                            }}
                          >
                            kopyala
                          </button>
                        </div>
                      </td>

                      <td className="align-middle">
                        <Badge variant="outline">{orgName(x.organizationId)}</Badge>
                      </td>

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
                    <td colSpan={5} className="py-6 text-center text-muted-foreground">
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
                        E-posta: <span className="underline">{x.email}</span>
                      </div>
                      <div className="mt-1">
                        <Badge variant="outline">{orgName(x.organizationId)}</Badge>
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
