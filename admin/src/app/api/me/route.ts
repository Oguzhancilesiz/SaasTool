// src/app/api/me/route.ts
import { NextResponse } from "next/server";
import { cookies } from "next/headers";

export async function GET() {
  const base = process.env.BACKEND_URL ?? "https://localhost:7298";
  const token = cookies().get("auth_token")?.value;

  if (!token) {
    return NextResponse.json({ error: "unauthorized" }, { status: 401 });
  }

  const res = await fetch(`${base}/api/v1/auth/me`, {
    headers: { authorization: `Bearer ${token}` },
    cache: "no-store",
  });

  if (!res.ok) {
    // Token süresi geçmiş olabilir. Cookie’yi temizleyelim ki loop olmasın.
    cookies().delete("auth_token");
    return NextResponse.json({ error: "unauthorized" }, { status: 401 });
  }

  const data = await res.json();
  return NextResponse.json(data);
}
