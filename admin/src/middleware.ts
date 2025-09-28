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
  const needsAuth = PROTECTED.some((p) => pathname === p || pathname.startsWith(p + "/"));
  if (!needsAuth) return NextResponse.next();

  const token = req.cookies.get("token")?.value;
  if (!token) {
    const url = new URL("/login", req.url);
    url.searchParams.set("returnUrl", pathname);
    return NextResponse.redirect(url);
  }
  return NextResponse.next();
}

export const config = {
  matcher: ["/((?!_next|api|favicon.ico).*)"],
};
