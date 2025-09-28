// ------------------------------
// 2) src/lib/format.ts
// ------------------------------
export function fmtNumber(n: number | null | undefined, fractionDigits = 0) {
if (n == null || isNaN(n as any)) return "-";
return new Intl.NumberFormat("tr-TR", { maximumFractionDigits: fractionDigits }).format(Number(n));
}
export function fmtMoneyTRY(n: number | null | undefined) {
if (n == null || isNaN(n as any)) return "-";
return new Intl.NumberFormat("tr-TR", { style: "currency", currency: "TRY", maximumFractionDigits: 2 }).format(Number(n));
}
export function startOfUTC(date: Date) {
return new Date(Date.UTC(date.getUTCFullYear(), date.getUTCMonth(), date.getUTCDate()));
}