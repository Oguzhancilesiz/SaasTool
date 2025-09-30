"use client";

import { create } from "zustand";
import { persist, createJSONStorage } from "zustand/middleware";

type OrgState = {
  orgId: string | null;
  setOrg: (id: string) => void;
  clearOrg: () => void;
};

export const useOrgStore = create<OrgState>()(
  persist(
    (set) => ({
      orgId: null,
      setOrg: (id) => set({ orgId: id }),
      clearOrg: () => set({ orgId: null }),
    }),
    {
      name: "org-store",
      storage: createJSONStorage(() => (typeof window !== "undefined" ? window.localStorage : undefined)),
      // SSR sırasında localStorage yok; hydration sapıtmasın diye serialize edenler defaultsuz.
      skipHydration: true,
    }
  )
);
