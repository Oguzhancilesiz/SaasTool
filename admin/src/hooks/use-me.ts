"use client";
import useSWR from "swr";
import type { AppUser } from "@/types/auth";

const fetcher = (url: string) =>
  fetch(url, { cache: "no-store" }).then(r => {
    if (!r.ok) throw new Error("unauthorized");
    return r.json();
  });

export function useMe() {
  const { data, error, isLoading, mutate } = useSWR<AppUser>("/api/me", fetcher, {
    revalidateOnFocus: false
  });
  return {
    user: data,
    isLoading,
    isError: !!error,
    refresh: mutate
  };
}
