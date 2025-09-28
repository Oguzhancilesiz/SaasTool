export const CurrencyMap = ["TRY","USD","EUR"] as const;
export const BillingPeriodMap = ["Aylık","Yıllık"] as const; // backend daha fazla desteklerse genişlet
export const ProviderMap = ["Manual","Stripe","Iyzico","PayPal"]; // örnek
export const PeriodUnitMap = ["Gün","Hafta","Ay","Çeyrek","Yıl"];

export const yesno = (b?: boolean) => (b ? "Evet" : "Hayır");
export const fmt = (d?: string|null) => (d ? new Date(d).toLocaleString("tr-TR") : "-");
export const money = (v:number, cur:number=0) => `${v.toLocaleString("tr-TR", {maximumFractionDigits:2})} ${CurrencyMap[cur] ?? ""}`;
