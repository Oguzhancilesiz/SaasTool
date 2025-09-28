export function toCsv<T extends object>(rows: T[], headers?: Record<string,string>) {
  const cols = headers ? Object.keys(headers) : Object.keys(rows[0] ?? {});
  const head = (headers ? Object.values(headers) : cols).join(";");
  const esc = (v:any) => {
    if (v==null) return "";
    const s = String(v).replace(/"/g,'""');
    return /[;"\n]/.test(s) ? `"${s}"` : s;
  };
  const body = rows.map(r => cols.map(c => esc((r as any)[c])).join(";")).join("\n");
  return head + "\n" + body;
}

export function downloadCsv(filename: string, csv: string) {
  const blob = new Blob([new TextEncoder().encode(csv)], { type: "text/csv;charset=utf-8;" });
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a"); a.href = url; a.download = filename; a.click();
  setTimeout(()=>URL.revokeObjectURL(url), 1000);
}
