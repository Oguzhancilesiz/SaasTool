"use client";
import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { usePermissions } from "@/hooks/use-permissions";

export default function RequirePermission({ need, children }:{ need: string | string[]; children: React.ReactNode }) {
  const { can } = usePermissions();
  const router = useRouter();
  const ok = can(need);

  useEffect(() => { if (!ok) router.replace("/dashboard"); }, [ok, router]);
  if (!ok) return null;
  return <>{children}</>;
}
