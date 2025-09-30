"use client";
import Image from "next/image";
import { RoleBadges } from "@/components/role-badges";
import type { AppUser } from "@/types/auth";

export function UserChip({ user }: { user: AppUser }) {
  return (
    <div className="flex items-center gap-3">
      <div className="text-right leading-tight hidden sm:block">
        <div className="text-sm font-medium">{user.fullName || user.email}</div>
        <div className="text-[11px] text-muted-foreground">
          {user.organization?.name ?? "Orgsız"} · {user.app?.name ?? "Appsız"}
        </div>
        <div className="mt-0.5"><RoleBadges roles={user.roles} /></div>
      </div>
      <div className="h-8 w-8 overflow-hidden rounded-full bg-muted flex items-center justify-center">
        {user.avatarUrl ? (
          <Image src={user.avatarUrl} alt="avatar" width={32} height={32} />
        ) : (
          <span className="text-xs">
            {(user.fullName ?? user.email ?? "?").slice(0, 1).toUpperCase()}
          </span>
        )}
      </div>
    </div>
  );
}
