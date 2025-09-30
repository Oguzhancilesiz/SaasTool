"use client";
import { useEffect, useState } from "react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Popover,
  PopoverTrigger,
  PopoverContent,
} from "@/components/ui/popover";
import { readToken, decodeJwt } from "@/lib/auth";
import { LogOut, Copy, Mail, Shield } from "lucide-react";

type Claims = {
  email?: string;
  name?: string;
  // asp.net jwt rolleri farklı claim adlarıyla gelebilir:
  role?: string | string[];
  roles?: string[];
  ["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]?: string | string[];
  organizationName?: string;
  appName?: string;
  [k: string]: any;
};

function normalizeRoles(c: Claims): string[] {
  const raw =
    c.roles ??
    c.role ??
    c["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ??
    [];
  return Array.isArray(raw) ? raw : raw ? [raw] : [];
}

export function HeaderUser() {
  const [name, setName] = useState<string | undefined>();
  const [email, setEmail] = useState<string | undefined>();
  const [roles, setRoles] = useState<string[]>([]);
  const [org, setOrg] = useState<string | undefined>();
  const [app, setApp] = useState<string | undefined>();

  useEffect(() => {
    const t = readToken();
    if (!t) return;
    const claims = decodeJwt<Claims>(t);
    if (!claims) return;
    setName(claims.name);
    setEmail(claims.email);
    setRoles(normalizeRoles(claims));
    setOrg(claims.organizationName ?? claims.organization?.name);
    setApp(claims.appName ?? claims.app?.name);
  }, []);

  if (!email && !name) return null;

  const display = name || email || "User";

  async function copyMail() {
    if (!email) return;
    try {
      await navigator.clipboard.writeText(email);
    } catch {
      /* boş ver, dramatik olsun */
    }
  }

  async function logout() {
    // Eğer /api/auth/logout route’un varsa oraya vur.
    try {
      await fetch("/api/auth/logout", { method: "POST" });
    } catch {}
    // Token’ı localStorage’da tutuyorsan, bari temizle:
    if (typeof window !== "undefined") {
      localStorage.removeItem("access_token");
      localStorage.removeItem("token");
      window.location.href = "/login";
    }
  }

  return (
    <Popover>
      <PopoverTrigger asChild>
        <Button
          variant="ghost"
          className="h-9 px-2 data-[state=open]:bg-muted"
          title="Kullanıcı"
        >
          <div className="flex flex-col items-end leading-tight mr-1">
            <div className="text-sm font-medium">{display}</div>
            {email && name && (
              <div className="text-[11px] text-muted-foreground">{email}</div>
            )}
          </div>
          {/* avatar’sız minimal ikon gibi dursun */}
          <div className="ml-2 h-8 w-8 rounded-full bg-muted grid place-items-center text-xs">
            {(display ?? "?").slice(0, 1).toUpperCase()}
          </div>
          <span className="sr-only">Kullanıcı menüsünü aç</span>
        </Button>
      </PopoverTrigger>

      <PopoverContent
        align="end"
        sideOffset={8}
        className="w-80 p-3"
      >
        <div className="space-y-3">
          <div>
            <div className="text-sm font-semibold">{display}</div>
            {email && (
              <button
                onClick={copyMail}
                className="mt-0.5 inline-flex items-center gap-1 text-xs text-muted-foreground hover:underline"
              >
                <Mail size={12} /> {email}
                <Copy size={12} className="opacity-70" />
              </button>
            )}
            {(org || app) && (
              <div className="mt-1 text-[11px] text-muted-foreground">
                {org ? org : "Orgsız"} · {app ? app : "Appsız"}
              </div>
            )}
          </div>

          {roles.length > 0 && (
            <div>
              <div className="mb-1 flex items-center gap-1 text-xs text-muted-foreground">
                <Shield size={12} /> Roller
              </div>
              <div className="flex flex-wrap gap-1">
                {roles.map((r) => (
                  <Badge key={r} variant="secondary" className="uppercase">
                    {r}
                  </Badge>
                ))}
              </div>
            </div>
          )}

          <div className="flex items-center justify-end gap-2 pt-2 border-t">
            <Button variant="outline" size="sm" onClick={copyMail}>
              <Copy size={14} className="mr-1" /> Maili kopyala
            </Button>
            <Button variant="destructive" size="sm" onClick={logout}>
              <LogOut size={14} className="mr-1" /> Çıkış
            </Button>
          </div>
        </div>
      </PopoverContent>
    </Popover>
  );
}
