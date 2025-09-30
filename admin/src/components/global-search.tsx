"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import { cn } from "@/lib/utils";
import { Search, Loader2 } from "lucide-react";

type SearchItem = {
  title: string;
  subtitle?: string;
  href: string;
  type?: string; // "page" | "customer" | "invoice" | ...
};

const SHORTCUT_TXT = typeof navigator !== "undefined" && /(Mac|iPhone|iPad)/i.test(navigator.platform) ? "⌘K" : "Ctrl K";

export function GlobalSearch() {
  const r = useRouter();
  const [open, setOpen] = useState(false);
  const [q, setQ] = useState("");
  const [loading, setLoading] = useState(false);
  const [items, setItems] = useState<SearchItem[]>([]);
  const [index, setIndex] = useState(-1);
  const inputRef = useRef<HTMLInputElement>(null);
  const listRef = useRef<HTMLDivElement>(null);
  const controllerRef = useRef<AbortController | null>(null);

  // Basit debounce
  const debouncedQ = useDebounce(q, 200);

  // Kısayollar: / ile odak, Ctrl/Cmd+K ile aç/kapa
  useEffect(() => {
    function onKey(e: KeyboardEvent) {
      if (e.key === "/" && !e.metaKey && !e.ctrlKey && !e.altKey) {
        // input veya editable içindeysek bozma
        const tag = (e.target as HTMLElement)?.tagName?.toLowerCase();
        const editable = (e.target as HTMLElement)?.getAttribute?.("contenteditable");
        if (tag === "input" || tag === "textarea" || editable === "true") return;
        e.preventDefault();
        setOpen(true);
        requestAnimationFrame(() => inputRef.current?.focus());
      }
      const mod = e.metaKey || e.ctrlKey;
      if (mod && (e.key.toLowerCase() === "k")) {
        e.preventDefault();
        setOpen(o => !o);
        requestAnimationFrame(() => inputRef.current?.focus());
      }
      if (e.key === "Escape") {
        setOpen(false);
        setQ("");
        setItems([]);
        setIndex(-1);
      }
    }
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, []);

  // Arama
  useEffect(() => {
    if (!open) return;
    if (!debouncedQ?.trim()) {
      setItems([]);
      setLoading(false);
      return;
    }
    controllerRef.current?.abort();
    const ac = new AbortController();
    controllerRef.current = ac;
    setLoading(true);
    fetch(`/api/search?q=${encodeURIComponent(debouncedQ)}`, { signal: ac.signal, cache: "no-store" })
      .then(async (res) => {
        if (!res.ok) throw new Error("search_failed");
        const data = (await res.json()) as { items: SearchItem[] };
        setItems(data.items || []);
        setIndex(data.items?.length ? 0 : -1);
      })
      .catch(() => {
        if (!ac.signal.aborted) {
          setItems([]);
          setIndex(-1);
        }
      })
      .finally(() => setLoading(false));
  }, [debouncedQ, open]);

  // Ok tuşları ve Enter
  useEffect(() => {
    if (!open) return;
    function onKey(e: KeyboardEvent) {
      if (!items.length) return;
      if (e.key === "ArrowDown") {
        e.preventDefault();
        setIndex(i => (i + 1) % items.length);
        scrollActiveIntoView();
      }
      if (e.key === "ArrowUp") {
        e.preventDefault();
        setIndex(i => (i - 1 + items.length) % items.length);
        scrollActiveIntoView();
      }
      if (e.key === "Enter" && index >= 0) {
        e.preventDefault();
        const it = items[index];
        if (it?.href) {
          setOpen(false);
          setQ("");
          setItems([]);
          r.push(it.href);
        }
      }
    }
    function scrollActiveIntoView() {
      const el = listRef.current?.querySelector<HTMLElement>('[data-active="true"]');
      el?.scrollIntoView({ block: "nearest" });
    }
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [open, items, index, r]);

  // Kutu focus state
  useEffect(() => {
    if (!open) return;
    requestAnimationFrame(() => inputRef.current?.focus());
  }, [open]);

  return (
    <div className="relative w-full max-w-sm md:max-w-md">
      {/* Compact button + input */}
      <div
        className={cn(
          "group flex items-center gap-2 rounded-xl border px-3 py-1.5 text-sm transition-colors",
          "bg-background hover:bg-muted/50"
        )}
        onClick={() => {
          setOpen(true);
          requestAnimationFrame(() => inputRef.current?.focus());
        }}
        role="button"
        aria-label="Global Search"
      >
        <Search size={16} className="opacity-70" />
        <input
          ref={inputRef}
          value={q}
          onChange={(e) => { setQ(e.target.value); setOpen(true); }}
          placeholder="Ara...  /  veya  Ctrl/Cmd+K"
          className="w-full bg-transparent outline-none placeholder:text-muted-foreground"
          onFocus={() => setOpen(true)}
        />
        <kbd className="ml-auto hidden sm:inline-flex items-center rounded border bg-muted px-1.5 py-0.5 text-[10px] text-muted-foreground">
          {SHORTCUT_TXT}
        </kbd>
      </div>

      {/* Results popover */}
      {open && (
        <div className="absolute left-0 right-0 z-50 mt-2 overflow-hidden rounded-xl border bg-popover shadow-xl">
          {/* status */}
          <div className="flex items-center gap-2 px-3 py-2 text-xs text-muted-foreground border-b">
            {loading ? (
              <>
                <Loader2 size={14} className="animate-spin" />
                Aranıyor…
              </>
            ) : (
              <>Sonuçlar</>
            )}
          </div>

          <div ref={listRef} className="max-h-[50vh] overflow-auto py-1">
            {(!items || items.length === 0) && !loading ? (
              <div className="px-3 py-4 text-sm text-muted-foreground">Bir şey bulunamadı.</div>
            ) : (
              items.map((it, i) => {
                const active = i === index;
                return (
                  <div
                    key={`${it.href}-${i}`}
                    data-active={active ? "true" : undefined}
                    onMouseEnter={() => setIndex(i)}
                    onMouseDown={(e) => {
                      // mousedown'da trigger edip focus kaybını önemsemeyelim
                      e.preventDefault();
                    }}
                    onClick={() => {
                      setOpen(false);
                      setQ("");
                      setItems([]);
                      r.push(it.href);
                    }}
                    className={cn(
                      "cursor-pointer px-3 py-2 text-sm",
                      active ? "bg-accent text-accent-foreground" : "hover:bg-muted"
                    )}
                    role="option"
                    aria-selected={active}
                  >
                    <div className="font-medium leading-none">{it.title}</div>
                    {it.subtitle ? (
                      <div className="mt-0.5 text-xs text-muted-foreground">{it.subtitle}</div>
                    ) : null}
                  </div>
                );
              })
            )}
          </div>

          <div className="flex items-center justify-between gap-2 border-t px-3 py-2 text-[11px] text-muted-foreground">
            <span className="hidden sm:inline">Ok tuşlarıyla gez, Enter ile aç, Esc ile kapat</span>
            <span className="inline sm:hidden">{SHORTCUT_TXT}</span>
          </div>
        </div>
      )}
    </div>
  );
}

function useDebounce<T>(value: T, delay = 250) {
  const [v, setV] = useState(value);
  useEffect(() => {
    const t = setTimeout(() => setV(value), delay);
    return () => clearTimeout(t);
  }, [value, delay]);
  return v;
}
