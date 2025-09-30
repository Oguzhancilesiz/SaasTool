// src/app/(protected)/apps/page.tsx
"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import { api } from "@/lib/api";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { ListToolbar } from "@/components/list-toolbar";
import { Pagination } from "@/components/pagination";
import { ConfirmButton } from "@/components/confirm-button";
import { toCsv, downloadCsv } from "@/lib/csv";
import { AppDto, Paged } from "@/types/dto";
import { AppForm } from "@/components/forms/app-form";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";

type SortKey = "code" | "name" | "isEnabled";
type SortDir = "asc" | "desc";

export default function AppsPage() {
  const [page, setPage] = useState(1);
  const pageSize = 20;

  // Arama: toolbar’dan gelen değer -> debounce ile q’ya yazılır
  const [searchTerm, setSearchTerm] = useState("");
  const [q, setQ] = useState("");

  const [data, setData] = useState<Paged<AppDto> | null>(null);
  const [loading, setLoading] = useState(false);
  const [err, setErr] = useState<string | null>(null);

  const [sortKey, setSortKey] = useState<SortKey>("name");
  const [sortDir, setSortDir] = useState<SortDir>("asc");

  const [selected, setSelected] = useState<Set<string>>(new Set());

  // Aynı anda birden çok fetch’i iptal edebilmek için
  const abortRef = useRef<AbortController | null>(null);

  // Debounce arama
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
      const res = await api.get<Paged<AppDto>>(`/apps?${params}`, {
        signal: controller.signal as AbortSignal,
      });
      setData(res.data);
      // sayfa değişince seçimleri sıfırla
      setSelected(new Set());
    } catch (e: any) {
      if (e?.name === "CanceledError" || e?.code === "ERR_CANCELED") {
        // iptal edildi, sustur
      } else {
        setErr(e?.response?.data?.message || e?.message || "Liste yüklenemedi.");
        toast.error("Liste yüklenemedi");
      }
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load().catch(() => {});
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [page, q]);

  const toggleEnabled = async (id: string, next: boolean) => {
    try {
      // Optimistic UI
      setData(d =>
        d
          ? {
              ...d,
              items: d.items.map(x => (x.id === id ? { ...x, isEnabled: next } : x)),
            }
          : d
      );
      await api.patch(`/apps/${id}`, { isEnabled: next });
      toast.success(next ? "Aktifleştirildi" : "Pasifleştirildi");
    } catch {
      // geri al
      setData(d =>
        d
          ? {
              ...d,
              items: d.items.map(x => (x.id === id ? { ...x, isEnabled: !next } : x)),
            }
          : d
      );
      toast.error("Güncellenemedi");
    }
  };

  const del = async (id: string) => {
    await api.delete(`/apps/${id}`);
    toast.success("Silindi");
    await load();
  };

  const delMany = async (ids: string[]) => {
    if (!ids.length) return;
    try {
      await api.delete(`/apps`, { data: { ids } as any });
      toast.success(`${ids.length} kayıt silindi`);
      await load();
    } catch {
      toast.error("Toplu silme başarısız");
    }
  };

  // CSV: eğer seçim varsa sadece seçimi, yoksa sayfadaki hepsini
  const exportCsv = () => {
    const src = (data?.items ?? []).filter(x => !selected.size || selected.has(x.id));
    const rows = src.map(x => ({ Kod: x.code, Ad: x.name, Aktif: x.isEnabled ? "Evet" : "Hayır" }));
    if (!rows.length) return toast.info("İhracat için kayıt yok");
    downloadCsv("apps.csv", toCsv(rows));
  };

  // Sayfa içi sıralama (server-side sıralama yoksa)
  const items = useMemo(() => {
    const arr = [...(data?.items ?? [])];
    arr.sort((a, b) => {
      const A = sortKey === "isEnabled" ? Number(a.isEnabled) : String(a[sortKey] ?? "").toLowerCase();
      const B = sortKey === "isEnabled" ? Number(b.isEnabled) : String(b[sortKey] ?? "").toLowerCase();
      // @ts-ignore
      if (A < B) return sortDir === "asc" ? -1 : 1;
      // @ts-ignore
      if (A > B) return sortDir === "asc" ? 1 : -1;
      return 0;
    });
    return arr;
  }, [data?.items, sortKey, sortDir]);

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
          <CardTitle>Yeni Uygulama</CardTitle>
        </CardHeader>
        <CardContent>
          <AppForm onCreated={load} />
        </CardContent>
      </Card>

      {/* List */}
      <Card>
        <CardHeader>
          <div className="flex flex-col gap-2 sm:flex-row sm:items-end sm:justify-between">
            <div>
              <CardTitle>Uygulamalar</CardTitle>
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
          {/* Masaüstü tablo */}
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
                  <Th label="Kod" active={sortKey === "code"} dir={sortDir} onClick={() => onSort("code")} />
                  <Th label="Ad" active={sortKey === "name"} dir={sortDir} onClick={() => onSort("name")} />
                  <Th label="Aktif" active={sortKey === "isEnabled"} dir={sortDir} onClick={() => onSort("isEnabled")} />
                  <th className="w-48 text-right">
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
                      <td><div className="h-4 w-24 rounded bg-muted" /></td>
                      <td><div className="h-4 w-40 rounded bg-muted" /></td>
                      <td><div className="h-5 w-14 rounded bg-muted" /></td>
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

                      <td className="py-2 align-middle">
                        <div className="inline-flex items-center gap-2">
                          <code className="rounded bg-muted px-1.5 py-0.5 text-xs">{x.code}</code>
                          <button
                            type="button"
                            className="text-xs text-muted-foreground hover:underline"
                            onClick={() => {
                              navigator.clipboard?.writeText(x.code);
                              toast.success("Kod kopyalandı");
                            }}
                          >
                            kopyala
                          </button>
                        </div>
                      </td>

                      <td className="align-middle">{x.name}</td>

                      <td className="align-middle">
                        <Badge variant={x.isEnabled ? "default" : "outline"} className={cn(!x.isEnabled && "text-muted-foreground")}>
                          {x.isEnabled ? "Evet" : "Hayır"}
                        </Badge>
                      </td>

                      <td className="text-right align-middle">
                        <div className="inline-flex items-center gap-2">
                          <Button
                            size="sm"
                            variant="outline"
                            onClick={() => toggleEnabled(x.id, !x.isEnabled)}
                          >
                            {x.isEnabled ? "Pasifleştir" : "Aktifleştir"}
                          </Button>
                          <ConfirmButton onConfirm={() => del(x.id)} size="sm">
                            Sil
                          </ConfirmButton>
                        </div>
                      </td>
                    </tr>
                  ))}

                {/* Empty */}
                {!loading && !items.length && (
                  <tr>
                    <td colSpan={5} className="py-8 text-center text-muted-foreground">
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
                        Kod: <code className="bg-muted px-1 py-0.5 rounded">{x.code}</code>
                      </div>
                      <div className="mt-1">
                        <Badge variant={x.isEnabled ? "default" : "outline"}>
                          {x.isEnabled ? "Aktif" : "Pasif"}
                        </Badge>
                      </div>
                    </div>
                    <div className="flex flex-col gap-2">
                      <Button size="sm" variant="outline" onClick={() => toggleEnabled(x.id, !x.isEnabled)}>
                        {x.isEnabled ? "Pasifleştir" : "Aktifleştir"}
                      </Button>
                      <ConfirmButton size="sm" onConfirm={() => del(x.id)}>Sil</ConfirmButton>
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

/* ---------- küçük başlık butonu ---------- */
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
