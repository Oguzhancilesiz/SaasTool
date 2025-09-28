// src/app/(protected)/layout.tsx
import type { ReactNode } from "react";
import { AppShell } from "@/components/app-shell";

export default function ProtectedLayout({ children }: { children: ReactNode }) {
  return <AppShell>{children}</AppShell>;
}
