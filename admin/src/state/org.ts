
// ==============================
// Pro Dashboard & Layout Upgrade
// Files included in this single snippet:
// 1) src/state/org.ts
// 2) src/lib/format.ts
// 3) src/components/kpi-card.tsx
// 4) src/components/charts.tsx
// 5) src/components/date-range.tsx
// 6) src/components/org-switcher.tsx
// 7) src/components/app-switcher.tsx
// 8) src/components/recent-events.tsx
// 9) src/components/health-cards.tsx
// 10) src/components/top-customers.tsx (optional demo)
// 11) src/components/app-shell.tsx (enhanced)
// 12) src/app/(protected)/dashboard/page.tsx (full pro dashboard)
//
// Notes:
// - Uses your existing axios `api` and auth.
// - Reads/sets selected orgId & appId via localStorage + jotai for global access.
// - Charts: recharts; responsive; dark-mode friendly using CSS vars.
// - Date presets: 7d, 30d, QTD, YTD, Custom.
// - KPIs animate and show delta vs previous period when available.
// - Replace your existing files with these or merge as needed.
// ==============================
// ------------------------------
// 1) src/state/org.ts
// ------------------------------
"use client";
import { atom } from "jotai";


export type OrgState = { orgId: string | null; appId: string | null };


const initial: OrgState = {
orgId: typeof window !== "undefined" ? localStorage.getItem("orgId") : null,
appId: typeof window !== "undefined" ? localStorage.getItem("appId") : null,
};


export const orgAtom = atom<OrgState>(initial);


// helpers to persist to localStorage
export const setOrgLocal = (orgId: string | null) => {
if (typeof window === "undefined") return;
if (orgId) localStorage.setItem("orgId", orgId); else localStorage.removeItem("orgId");
};
export const setAppLocal = (appId: string | null) => {
if (typeof window === "undefined") return;
if (appId) localStorage.setItem("appId", appId); else localStorage.removeItem("appId");
};