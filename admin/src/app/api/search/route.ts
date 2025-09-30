import { NextResponse } from "next/server";

const SAMPLE = [
  { title: "Dashboard", subtitle: "Genel görünüm", href: "/dashboard", type: "page" },
  { title: "Uygulamalar", subtitle: "App listesi", href: "/apps", type: "page" },
  { title: "Planlar", subtitle: "Faturalama planları", href: "/plans", type: "page" },
  { title: "Müşteriler", subtitle: "CRM", href: "/customers", type: "page" },
  { title: "Abonelikler", subtitle: "Aktif/pasif abonelikler", href: "/subscriptions", type: "page" },
];

export async function GET(req: Request) {
  const { searchParams } = new URL(req.url);
  const q = (searchParams.get("q") || "").toLowerCase().trim();
  if (!q) return NextResponse.json({ items: [] });

  const items = SAMPLE.filter(x =>
    x.title.toLowerCase().includes(q) ||
    x.subtitle?.toLowerCase().includes(q)
  ).slice(0, 10);

  return NextResponse.json({ items });
}
