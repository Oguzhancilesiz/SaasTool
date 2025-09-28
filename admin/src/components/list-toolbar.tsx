"use client";
import { useState } from "react";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";

export function ListToolbar({ onSearch, onRefresh, onExport }: { onSearch:(s:string)=>void; onRefresh:()=>void; onExport:(fmt:"csv")=>void; }) {
  const [q,setQ] = useState("");
  return (
    <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
      <div className="flex gap-2">
        <Input placeholder="Ara..." value={q} onChange={e=>setQ(e.target.value)} onKeyDown={e=>e.key==="Enter" && onSearch(q)} />
        <Button variant="secondary" onClick={()=>onSearch(q)}>Ara</Button>
        <Button variant="outline" onClick={onRefresh}>Yenile</Button>
      </div>
      <div className="flex gap-2">
        <Button variant="outline" onClick={()=>onExport("csv")}>CSV</Button>
      </div>
    </div>
  );
}
