// src/components/app-shell.tsx
"use client";

import { useEffect, useMemo, useState, type ReactNode } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { cn } from "@/lib/utils";

import { OrgSwitcher } from "@/components/org-switcher";
import { AppSwitcher } from "@/components/app-switcher";
import { UserMenu } from "@/components/user-menu";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import { HeaderUser } from "@/components/header-user";
import { GlobalSearch } from "@/components/global-search";

import {
  LayoutDashboard, Boxes, ListChecks, Settings2, Building2, Users2, Receipt,
  CreditCard, BarChart2, Menu, Rocket, KeyIcon,
} from "lucide-react";

type Org = { id: string; name: string };
type AppItem = { id: string; name: string };

const NAV: Array<{ href: string; label: string; icon: React.ComponentType<{ size?: number }> }> = [
  { href: "/dashboard", label: "Dashboard", icon: LayoutDashboard },
  { href: "/apps", label: "Uygulamalar", icon: Boxes },
  { href: "/plans", label: "Planlar", icon: ListChecks },
  { href: "/features", label: "Özellikler", icon: Settings2 },
  { href: "/organizations", label: "Organizasyonlar", icon: Building2 },
  { href: "/customers", label: "Müşteriler", icon: Users2 },
  { href: "/subscriptions", label: "Abonelikler", icon: BarChart2 },
  { href: "/usage", label: "Kullanım", icon: BarChart2 },
  { href: "/invoices", label: "Faturalar", icon: Receipt },
  { href: "/payments", label: "Ödemeler", icon: CreditCard },
  { href: "/api-keys", label: "API Keys", icon: KeyIcon },
];

// Full-bleed + hafif yan boşluklar (gutter)
const SHELL = "w-full max-w-none px-2 sm:px-3 md:px-4";
// Varsayılan grid (geniş sidebar)
const GRID = "grid gap-0 md:grid-cols-[280px_minmax(0,1fr)]";

/* ------------ Mini yardımcılar (UI bozmaz) ------------ */
function useRouteProgress() {
  const pathname = usePathname();
  useEffect(() => {
    const el = document.getElementById("topbar-progress");
    if (!el) return;
    el.classList.remove("w-0");
    el.classList.add("w-1/2");
    const t = setTimeout(() => {
      el.classList.remove("w-1/2");
      el.classList.add("w-full");
      setTimeout(() => el.classList.add("w-0"), 180);
    }, 180);
    return () => clearTimeout(t);
  }, [pathname]);
}

function OnlineBadge() {
  const [on, setOn] = useState(true);
  useEffect(() => {
    const f1 = () => setOn(true);
    const f0 = () => setOn(false);
    window.addEventListener("online", f1);
    window.addEventListener("offline", f0);
    setOn(navigator.onLine);
    return () => {
      window.removeEventListener("online", f1);
      window.removeEventListener("offline", f0);
    };
  }, []);
  return (
    <span
      className={cn(
        "text-xs px-2 py-0.5 rounded-full border",
        on ? "text-emerald-600 border-emerald-500/30" : "text-red-600 border-red-500/30"
      )}
      aria-live="polite"
    >
      {on ? "Online" : "Offline"}
    </span>
  );
}

function Breadcrumbs({ path }: { path: string }) {
  const parts = useMemo(() => path.split("/").filter(Boolean), [path]);
  let acc = "";
  return (
    <nav aria-label="breadcrumbs" className="hidden md:block text-xs text-muted-foreground">
      <ol className="flex items-center gap-2">
        <li>
          <Link href="/dashboard" className="hover:underline">Dashboard</Link>
        </li>
        {parts.map((p, i) => {
          acc += `/${p}`;
          return (
            <li key={i} className="flex items-center gap-2">
              <span>/</span>
              <Link href={acc} className="capitalize hover:underline">
                {p.replaceAll("-", " ")}
              </Link>
            </li>
          );
        })}
      </ol>
    </nav>
  );
}

/* ---------------- AppShell ---------------- */
export function AppShell({
  children,
  orgs,
  apps,
}: {
  children: ReactNode;
  orgs?: Org[];
  apps?: AppItem[]; // opsiyonel: gerçek liste varsa AppSwitcher'a verirsin
}) {
  useRouteProgress();

  const pathname = usePathname();
  const [open, setOpen] = useState(false);

  // Sidebar daraltma + kalıcılık
  const [collapsed, setCollapsed] = useState<boolean>(() => {
    if (typeof window === "undefined") return false;
    return window.localStorage.getItem("sidebar:collapsed") === "1";
  });
  useEffect(() => {
    if (typeof window !== "undefined") {
      window.localStorage.setItem("sidebar:collapsed", collapsed ? "1" : "0");
    }
  }, [collapsed]);

  // Daraltılmış grid
  const gridClass = collapsed
    ? "grid gap-0 md:grid-cols-[72px_minmax(0,1fr)]"
    : GRID;

  const safeOrgs = Array.isArray(orgs) ? orgs : [];
  const safeApps = Array.isArray(apps) ? apps : [];

  return (
    <div className="min-h-screen bg-gradient-to-b from-background to-background/60">
      {/* Üstte mini progress bar (route change) */}
      <div id="topbar-progress" className="fixed left-0 top-0 h-[2px] w-0 bg-primary transition-all duration-300 z-50" />

      {/* HEADER */}
      <header className="sticky top-0 z-40 border-b bg-background/90 backdrop-blur supports-[backdrop-filter]:bg-background/75">
        <div className={SHELL}>
          <div className={cn(gridClass, "h-14 items-center")}>
            {/* Sol kolon (sidebar hizası) */}
            <div className="hidden md:flex items-center gap-2">
              <button className="rounded-lg p-2 hover:bg-muted" aria-label="menu" onClick={() => setOpen(s => !s)}>
                <Menu size={18} />
              </button>
              <Link href="/dashboard" className="font-semibold inline-flex items-center gap-2">
                <Rocket size={18} /> SaaS Admin
              </Link>
            </div>

            {/* Sağ kolon (topbar) */}
            <div className="flex items-center gap-3 min-w-0">
              {/* Mobil logo+menu */}
              <div className="md:hidden inline-flex items-center gap-2">
                <button className="rounded-lg p-2 hover:bg-muted" aria-label="menu" onClick={() => setOpen(s => !s)}>
                  <Menu size={18} />
                </button>
                <Link href="/dashboard" className="font-semibold inline-flex items-center gap-2">
                  <Rocket size={18} /> SaaS Admin
                </Link>
              </div>

              <Separator orientation="vertical" className="mx-2 hidden md:block h-6" />

              {/* Breadcrumbs */}
              <Breadcrumbs path={pathname || "/"} />

              <div className="hidden md:flex items-center gap-2 ml-2">
                <OrgSwitcher orgs={safeOrgs} />
                <AppSwitcher apps={safeApps} />
              </div>

              <div className="mx-2 hidden md:block flex-1 min-w-0">
                <GlobalSearch />
              </div>

              <div className="ml-auto flex items-center gap-3">
                <OnlineBadge />
                <Badge variant="outline" className="hidden sm:inline-flex">v1</Badge>
                <HeaderUser />
                {/* UserMenu'yu istiyorsan aç: <UserMenu /> */}
              </div>
            </div>
          </div>
        </div>
      </header>

      {/* BODY */}
      <div className={SHELL}>
        <div className={cn(gridClass)}>
          {/* Sidebar */}
          <aside
            className={cn(
              "border-r bg-card/30 p-2 sm:p-3 md:p-3 md:sticky md:top-14 md:h-[calc(100dvh-3.5rem)]",
              open ? "block" : "hidden md:block"
            )}
          >
            <nav className="grid gap-1">
              {NAV.map(n => {
                const Icon = n.icon;
                const active = pathname === n.href || pathname?.startsWith(n.href + "/");
                return (
                  <Link
                    key={n.href}
                    href={n.href}
                    prefetch={false}
                    className={cn(
                      "flex items-center gap-2 rounded-xl px-3 py-2 text-sm transition-colors",
                      active ? "bg-primary/10 text-primary" : "text-muted-foreground hover:bg-muted hover:text-foreground"
                    )}
                    aria-current={active ? "page" : undefined}
                  >
                    <Icon size={18} />
                    <span className={cn("truncate", collapsed && "md:hidden")}>{n.label}</span>
                  </Link>
                );
              })}
            </nav>

            {/* Mobil switcher'lar */}
            <div className="mt-4 md:hidden"><OrgSwitcher orgs={safeOrgs} /></div>
            <div className="mt-2 md:hidden"><AppSwitcher apps={safeApps} /></div>

            {/* Collapse toggle */}
            <button
              className="mt-3 hidden md:block w-full rounded-lg border text-xs py-1 hover:bg-muted"
              onClick={() => setCollapsed(v => !v)}
              aria-pressed={collapsed}
            >
              {collapsed ? "Genişlet" : "Daralt"}
            </button>

            <div className="mt-auto hidden md:block pt-6">
              <Separator className="mb-3" />
              <div className="text-xs text-muted-foreground">
                Made with <span className="text-primary">♥</span> for admins who hate basic dashboards.
              </div>
            </div>
          </aside>

          {/* Main: yanlara hafif iç boşluk + taşma koruması */}
          <main className="min-w-0 px-2 sm:px-3 md:px-4 py-3 md:py-5">
            {children}
          </main>
        </div>
      </div>
    </div>
  );
}
