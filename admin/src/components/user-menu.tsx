"use client";

import { useAuth } from "@/hooks/use-auth";
import { Button } from "@/components/ui/button";
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuLabel, DropdownMenuSeparator, DropdownMenuTrigger } from "@/components/ui/dropdown-menu";

function initials(email: string) {
  const name = email?.split("@")[0] ?? "";
  const parts = name.split(/[.\-_]/).filter(Boolean);
  const first = parts[0]?.[0] ?? name[0] ?? "U";
  const second = parts[1]?.[0] ?? "";
  return (first + second).toUpperCase();
}

export function UserMenu() {
  const { email } = useAuth();

  const logout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("expiresAtUtc");
    document.cookie = "token=; Path=/; Max-Age=0";
    window.location.href = "/login";
  };

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="secondary" className="h-9 gap-2">
          <span className="inline-flex h-6 w-6 items-center justify-center rounded-full bg-primary text-primary-foreground text-xs">
            {initials(email)}
          </span>
          <span className="hidden sm:inline">{email || "Kullanıcı"}</span>
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-56">
        <DropdownMenuLabel>{email || "Kullanıcı"}</DropdownMenuLabel>
        <DropdownMenuSeparator />
        <DropdownMenuItem onClick={() => (window.location.href = "/dashboard")}>
          Dashboard
        </DropdownMenuItem>
        <DropdownMenuItem onClick={logout} className="text-red-600">
          Çıkış
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
