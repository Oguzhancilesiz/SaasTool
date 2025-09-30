// src/app/api/auth/login/route.ts
import { NextResponse } from "next/server";
import { cookies } from "next/headers";

export async function POST(req: Request) {
  const base = process.env.BACKEND_URL ?? "https://localhost:7298";
  const body = await req.json();

  const res = await fetch(`${base}/api/v1/auth/login`, {
    method: "POST",
    headers: { "content-type": "application/json" },
    body: JSON.stringify(body),
  });

  const data = await res.json().catch(() => ({}));

  if (!res.ok) {
    return NextResponse.json(
      { error: data?.message ?? "login_failed" },
      { status: res.status }
    );
  }

  const token: string | undefined = data?.token || data?.accessToken || data?.jwt;
  if (!token) {
    return NextResponse.json({ error: "token_missing_from_backend" }, { status: 500 });
  }

  const maxAge =
    typeof data?.expiresIn === "number" && data.expiresIn > 0
      ? data.expiresIn
      : 60 * 60 * 24 * 7;

  const isProd = process.env.NODE_ENV === "production";
  cookies().set("auth_token", token, {
    httpOnly: true,
    secure: isProd ? true : false,    // <<< kritik satır
    sameSite: "lax",
    path: "/",
    maxAge,
  });

  return NextResponse.json({ ok: true });
}
