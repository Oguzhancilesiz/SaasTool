SaaS Admin Paneli — Proje Özeti

Modern SaaS ürünleri için çok kiracılı (multi-tenant) bir yönetim paneli. Organizasyon, uygulama, plan, özellik, müşteri, abonelik, fatura, kullanım ve ödeme akışlarını uçtan uca yönetir. Proje; Next.js 15, TypeScript, React Server/Client Components mimarisi, Tailwind CSS, shadcn/ui, Zod + React Hook Form, Zustand/Jotai ve temiz API katmanı ile inşa edilmiştir. Backend tarafı .NET/EF Core tabanlı bir API’yi hedefler.

İçindekiler

Öne Çıkanlar

Teknolojiler ve Bağımlılıklar

Mimari ve Klasör Yapısı

Özellikler

Durum Yönetimi ve Hydration

Form Doğrulama ve UX

API ve Çevresel Değişkenler

Geliştirme ve Çalıştırma

Dağıtım (Deployment)

Kod Kalitesi

Güvenlik Notları

Yol Haritası

Lisans

Öne Çıkanlar

Next.js 15 (Turbopack) ile hızlı geliştirme ve modern RSC/CSR karışık mimari

Çok kiracılı tasarım: Organizasyon ve uygulama bağlamına duyarlı sayfalar

Org/App Switcher ile bağlam seçimi, Global Search

Düzgün SSR/CSR hydration pratikleri, Radix attribute farklarında korumalar

Zod + React Hook Form ile tip güvenli formlar, kullanıcı dostu UI

Zustand ile hafif ve kalıcı (persist) client state, Jotai ile auth atom

shadcn/ui + Tailwind bileşenleri, lucide-react ikon seti

CSV export, sortable tablolar, mobil kart görünümleri, skeleton durumlar

Toast/sonner ile aksiyon geri bildirimi

.NET tarafında EF Core tabanlı API ile KPI, grafik ve liste uçları (referans: DashboardService)

Teknolojiler ve Bağımlılıklar

Frontend

Framework: Next.js 15.5.4, React 18

Dil: TypeScript

Stil: Tailwind CSS

UI: shadcn/ui, Radix UI primitives, lucide-react ikonlar

Formlar: React Hook Form, Zod, @hookform/resolvers

Bildirimler: sonner

State: Zustand (persist + createJSONStorage), Jotai (auth atom)

İstek: api helper (Axios tabanlı bir client varsayımı)

Yardımcılar: tarih/sayı formatlama util’leri, CSV oluşturma ve indirme yardımcıları

Backend (hedef)

.NET + Entity Framework Core

Repository/Unit of Work pattern

DTO’lar: KpiDto, SeriesPointDto, BreakdownItemDto, FunnelDto, RecentEventDto, HealthDto, vb.

Örnek servis: DashboardService (KPI, MRR, churn, funnel, top customers, health)

src/
  app/
    (auth)/login/page.tsx            # Giriş sayfası (Remember me, show/hide password)
    (protected)/
      layout.tsx                     # Korumalı alan layout'u
      dashboard/page.tsx             # Örnek kontrol paneli
      apps/page.tsx                  # Uygulamalar
      plans/page.tsx                 # Planlar
      features/page.tsx              # Özellikler
      customers/page.tsx             # Müşteriler
      subscriptions/page.tsx         # Abonelikler
      invoices/page.tsx              # Faturalar
      usage/page.tsx                 # Kullanım kayıtları
  components/
    app-shell.tsx                    # Header + Sidebar + Content shell
    org-switcher.tsx                 # Organizasyon seçici (SSR/CSR guard'lı)
    app-switcher.tsx                 # Uygulama seçici (SSR/CSR guard'lı)
    header-user.tsx, user-menu.tsx   # Kullanıcı menüleri
    global-search.tsx                # Global arama
    list-toolbar.tsx                 # Liste araç çubuğu (search/refresh/export)
    pagination.tsx                   # Sayfalama bileşeni
    confirm-button.tsx               # Silme onayı vb.
    forms/
      app-form.tsx                   # Uygulama oluşturma
      feature-form.tsx               # Özellik oluşturma (App select ile)
      plan-form.tsx                  # Plan oluşturma (enum select, trial vs.)
      customer-form.tsx              # Müşteri oluşturma (Org select ile)
      subscription-form.tsx          # Abonelik oluşturma (Org/App/Plan/Customer select)
      invoice-form.tsx               # Fatura oluşturma (Org/Customer select, satır ekle-sil, toplamlar)
    ui/                              # shadcn/ui bileşenleri (button, input, card, label, etc.)
  lib/
    api.ts                           # Axios client ve interceptors
    enums.ts                         # Enum map/format yardımcıları (fmt, money, PeriodUnitMap, CurrencyMap)
    csv.ts                           # toCsv, downloadCsv yardımcıları
    utils.ts                         # cn, genel yardımcılar
  state/
    auth.ts                          # Jotai auth atom
  stores/
    org-store.ts                     # Zustand ile orgId persist store
Not: shadcn/ui bileşenleri proje içinde oluşturulur. Eksikse checkbox, tooltip gibi UI parçalarını eklemek gerekir.


Özellikler
Çerçeve ve Shell

AppShell: Full-bleed header, yapışkan sidebar, responsive grid. İçerikte min-w-0 ve gutter uygulaması.

OrgSwitcher / AppSwitcher: SSR/CSR hydration mismatch sorunlarına karşı mount guard, suppressHydrationWarning, controlled value stratejisi.

Oturum ve Login

Login sayfası: Zod doğrulama, şifre göster/gizle, beni hatırla, e-posta hatırla. Başarılı girişte token ve expire saklanır; atom güncellenir.

CRUD Ekranları

Apps / Features / Plans / Customers / Subscriptions / Invoices / Usage

ID yerine insan okuyabilir etiketler: Org · App · Plan · Customer gibi birleşik etiketler

Filtreler: org/app/plan/feature/subscription/period/tarih aralığı/durum

CSV export: Liste görünümündeki verilerden dosya indir

Sıralama: başlık tıklayınca artan/azalan

Skeleton: yüklenme sırasında iskelet satırlar

Mobil kart görünümü: küçük ekranlarda okunabilir kartlar

Pagination: basit, stateful

Dashboard API Örnekleri (.NET)

Günlük/MTD gelir, yeni müşteri, aktif abonelik, MRR ve churn hesaplamaları

Gelir ve abonelik serileri, plan kırılımı, funnel, son olaylar, health check

Durum Yönetimi ve Hydration

Zustand org-store: persist + createJSONStorage ile localStorage kullanır, SSR’de storage olmadığı için:

skipHydration: true

Switcher’larda mounted guard ile ilk kareyi sabit tutma

Radix data-placeholder gibi attribute farklarında suppressHydrationWarning kullanımı

Jotai: authAtom ile basit oturum durumu

Form Doğrulama ve UX

Zod şemaları ile React Hook Form entegre

Select/checkbox gibi input’larda insan odaklı seçimler, enum-sayı ezberi yok

Formlar arası bağlam: ör. organizasyon seçince müşteri listesi org’a göre filtreleniyor

Tarih girdilerinde datetime-local → ISO dönüşümü yardımcıları

Toast bildirimleri ve hatalar

API ve Çevresel Değişkenler

src/lib/api.ts Axios client’ı; tipik olarak:

const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_BASE_URL,
  withCredentials: true,
});


Gerekli değişkenler:

NEXT_PUBLIC_API_BASE_URL= https://api.example.com


Backend beklenen uçlar (örnekler):

GET /organizations, /apps, /plans, /features, /customers

GET /subscriptions, /invoices, /usage

POST /apps, /features, /plans, /customers, /subscriptions, /invoices

Dashboard: /dashboard/kpis, /dashboard/revenue-series, vb.

Not: Filtre parametreleri opsiyoneldir; backend desteklemezse yine çalışır, sadece daha geniş sonuç döner.

Geliştirme ve Çalıştırma

Ön koşullar:

Node 18+

pnpm veya npm

Kurulum:

pnpm install
# veya
npm install


Geliştirme:

pnpm dev
# NEXT_PUBLIC_API_BASE_URL set edilmiş olmalı


Build:

pnpm build
pnpm start

Dağıtım (Deployment)

Vercel önerilir; Next.js 15 ve RSC’ye uyumlu.

Ortam değişkenlerini (Environment Variables) panelden girin.

Cache/hydration soruları için:

suppressHydrationWarning yalnızca gerekli yerlerde

Client-only bileşenlerde "use client" ve mounted guard’ı

Kod Kalitesi

TypeScript strict mod önerilir.

UI: Tailwind + shadcn/ui desenleri; minimal ama üretim hazır bileşenler.

Yardımcılar: fmt, money, enum map’leri, toCsv, downloadCsv

Erişilebilirlik: Tüm interaktif öğelerde aria-label ve odak stilleri

Güvenlik Notları

Token saklama stratejisi dikkatli seçilmeli:

Demo’da localStorage + cookie; prod’da HttpOnly secure cookie önerilir.

API isteklerinde interceptors ile Authorization header eklenmesi

Form input’larında tip/doğrulama katmanı Zod ile sağlanır

Yol Haritası

 Dashboard grafiklerinin eklenmesi (Recharts/Visx)

 Gelişmiş arama ve kaydedilmiş filtreler

 RBAC/Yetkilendirme (roller, izinler)

 Çoklu dil desteği (i18n)

 E2E testler (Playwright) ve unit testler (Vitest/Jest)

 Webhook loglarının org bağlamına genişletilmesi

Lisans

MIT. İstediğin gibi kullan, yıldız ver, forkla, katkı gönder.
