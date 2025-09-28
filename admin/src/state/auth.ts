"use client";
import { atom } from "jotai";

export type AuthState = {
  token: string | null;
  expiresAtUtc: string | null;
};

export const authAtom = atom<AuthState>({
  token: typeof window !== "undefined" ? localStorage.getItem("token") : null,
  expiresAtUtc:
    typeof window !== "undefined" ? localStorage.getItem("expiresAtUtc") : null,
});
