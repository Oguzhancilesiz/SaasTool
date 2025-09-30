"use client";
import { createContext, useContext } from "react";

type AuthCtx = { permissions: string[] };
export const AuthContext = createContext<AuthCtx>({ permissions: [] });

export function useAuth() {
  return useContext(AuthContext);
}

export function Can({ permission, children }: { permission: string; children: React.ReactNode }) {
  const { permissions } = useAuth();
  if (!permission) return <>{children}</>;
  return permissions.includes(permission) ? <>{children}</> : null;
}
