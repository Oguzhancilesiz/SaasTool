"use client";
export function Pagination({ page, pageSize, total, onPage }: { page:number; pageSize:number; total:number; onPage:(p:number)=>void; }) {
  const pages = Math.max(1, Math.ceil(total / pageSize));
  return (
    <div className="flex items-center gap-2 text-sm">
      <button className="px-2 py-1 rounded border" disabled={page<=1} onClick={()=>onPage(page-1)}>Önceki</button>
      <span>Sayfa {page}/{pages}</span>
      <button className="px-2 py-1 rounded border" disabled={page>=pages} onClick={()=>onPage(page+1)}>Sonraki</button>
      <span className="text-muted-foreground ml-2">{total} kayıt</span>
    </div>
  );
}
