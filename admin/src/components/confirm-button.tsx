"use client";
import { useState } from "react";
import { Button } from "@/components/ui/button";

export function ConfirmButton({ onConfirm, children, variant="destructive", text="Emin misiniz?" }:{
  onConfirm:()=>Promise<void>|void; children:React.ReactNode; variant?:"default"|"secondary"|"destructive"|"outline"; text?:string;
}) {
  const [ask,setAsk]=useState(false);
  if (!ask) return <Button variant={variant} onClick={()=>setAsk(true)}>{children}</Button>;
  return (
    <div className="inline-flex items-center gap-2">
      <span className="text-sm">{text}</span>
      <Button size="sm" onClick={async()=>{ await onConfirm(); setAsk(false); }}>Evet</Button>
      <Button size="sm" variant="outline" onClick={()=>setAsk(false)}>Vazgeç</Button>
    </div>
  );
}
