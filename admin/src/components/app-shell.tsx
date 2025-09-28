// ------------------------------
// 11) src/components/app-shell.tsx (enhanced)
// ------------------------------
"use client";
import { useState } from "react";
import { cn } from "@/lib/utils";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { OrgSwitcher } from "@/components/org-switcher";
import { AppSwitcher } from "@/components/app-switcher";
import { UserMenu } from "@/components/user-menu";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import { Button } from "@/components/ui/button";
import {
  LayoutDashboard, Boxes, ListChecks, Settings2, Building2, Users2, Receipt,
  CreditCard, BarChart2, Menu, Rocket,
  KeyIcon
} from "lucide-react";

const NAV = [
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

export function AppShell({ children }: { children: React.ReactNode }) {
  const [open, setOpen] = useState(false);
  const pathname = usePathname();

  return (
    <div className="min-h-screen bg-gradient-to-b from-background to-background/60">
      {/* Topbar */}
      <header className="sticky top-0 z-40 border-b bg-background/85 backdrop-blur supports-[backdrop-filter]:bg-background/60">
        <div className="mx-auto flex h-14 max-w-7xl items-center gap-3 px-4">
          <button className="rounded-lg p-2 hover:bg-muted md:hidden" aria-label="menu" onClick={() => setOpen(s => !s)}>
            <Menu size={18} />
          </button>
          <Link href="/dashboard" className="font-semibold inline-flex items-center gap-2">
            <Rocket size={18} /> SaaS Admin
          </Link>
          <Separator orientation="vertical" className="mx-2 hidden md:block h-6" />
          <div className="hidden md:flex items-center gap-2">
            <OrgSwitcher />
            <AppSwitcher />
          </div>
          <div className="ml-auto flex items-center gap-2">
            <Badge variant="outline" className="hidden sm:inline-flex">v1</Badge>
            <UserMenu />
          </div>
        </div>
      </header>

      <div className="mx-auto grid max-w-7xl grid-cols-1 md:grid-cols-[240px_1fr]">
        {/* Sidebar */}
        <aside className={cn("border-r bg-card/30 p-3 md:sticky md:top-14 md:h-[calc(100dvh-3.5rem)]", open ? "block" : "hidden md:block")}> 
          <nav className="grid gap-1">
            {NAV.map(n => {
              const Icon = n.icon;
              const active = pathname === n.href || pathname?.startsWith(n.href + "/");
              return (
                <Link key={n.href} href={n.href} className={cn("flex items-center gap-2 rounded-xl px-3 py-2 text-sm transition-colors", active ? "bg-primary/10 text-primary" : "text-muted-foreground hover:bg-muted hover:text-foreground")}> 
                  <Icon size={18} />
                  <span>{n.label}</span>
                </Link>
              );
            })}
          </nav>
          <div className="mt-4 md:hidden"><OrgSwitcher /></div>
          <div className="mt-2 md:hidden"><AppSwitcher /></div>
          <div className="mt-auto hidden md:block pt-6">
            <Separator className="mb-3" />
            <div className="text-xs text-muted-foreground">Made with <span className="text-primary">♥</span> for admins who hate basic dashboards.</div>
          </div>
        </aside>

        {/* Content */}
        <main className="p-4 md:p-6">{children}</main>
      </div>
    </div>
  );
}