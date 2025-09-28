"use client";
import { useAtomValue } from "jotai";
import { authAtom } from "@/state/auth";
import { parseJwt } from "@/lib/jwt";

type JwtPayload = {
  email?: string;
  sub?: string;             // user id
  exp?: number;
  [k: string]: any;
};

export function useAuth() {
  const { token, expiresAtUtc } = useAtomValue(authAtom);
  const payload = parseJwt<JwtPayload>(token ?? undefined);
  const email = payload?.email ?? "";
  const userId = payload?.sub ?? "";
  return { token, expiresAtUtc, email, userId, payload };
}
