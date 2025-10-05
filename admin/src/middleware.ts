// middleware.ts
import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

const PROTECTED = [
  "/dashboard",
  "/apps",
  "/plans",
  "/features",
  "/organizations",
  "/customers",
  "/subscriptions",
  "/usage",
  "/invoices",
  "/payments",
];

export function middleware(req: NextRequest) {
  const { pathname } = req.nextUrl;

  // 1) Kökü login'e (ya da dashboard'a) yönlendir
  if (pathname === "/") {
    const url = req.nextUrl.clone();
    url.pathname = "/login"; // istersen "/dashboard"
    return NextResponse.redirect(url);
  }

  // 2) Korumalı sayfalar için auth kontrolü
  const needsAuth =
    PROTECTED.some(p => pathname === p || pathname.startsWith(p + "/"));

  if (!needsAuth) return NextResponse.next();

  const token = req.cookies.get("token")?.value;
  if (!token) {
    const url = req.nextUrl.clone();
    url.pathname = "/login";
    url.searchParams.set("returnUrl", pathname);
    return NextResponse.redirect(url);
  }

  return NextResponse.next();
}

// 3) Matcher: statikleri ve API'yi dışarıda bırak
export const config = {
  matcher: ["/((?!_next/static|_next/image|favicon.ico|api).*)"],
};
