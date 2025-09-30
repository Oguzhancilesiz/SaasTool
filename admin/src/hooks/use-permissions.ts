"use client";
import { useAuth } from "@/hooks/use-auth";
import { extractRoles, rolesToPermissions, hasPerm } from "@/lib/authz";
import { useMemo } from "react";

export function usePermissions() {
  const { payload } = useAuth();
  const roles = useMemo(() => extractRoles(payload), [payload]);
  const permissions = useMemo(() => rolesToPermissions(roles), [roles]);
  return { roles, permissions, can: (p: string | string[]) => hasPerm(permissions, p) };
}
