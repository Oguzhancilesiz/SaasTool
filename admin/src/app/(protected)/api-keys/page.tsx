"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { toast } from "sonner";

type Paged<T> = { total: number; page: number; pageSize: number; items: T[] };

type ApiKeyDto = {
  id: string;
  name: string;
  organizationId: string;
  appId: string;
  keyLast4: string;
  expiresAt?: string | null;
  isRevoked: boolean;
  createdDate: string;
};
type CreatePayload = { organizationId: string; appId: string; name: string; expiresAt?: string | null };
type CreatedResponse = { id: string; key: string; keyLast4: string };

export default function ApiKeysPage() {
  const [data, setData] = useState<Paged<ApiKeyDto> | null>(null);
  const [form, setForm] = useState<CreatePayload>({ organizationId: "", appId: "", name: "" });
  const [justCreated, setJustCreated] = useState<CreatedResponse | null>(null);

  const load = async () => {
    const qs = new URLSearchParams({ page: "1", pageSize: "50" }).toString();
    const res = await api.get<Paged<ApiKeyDto>>(`/api-keys?${qs}`);
    setData(res.data);
  };

  useEffect(() => { load().catch(console.error); }, []);

  const create = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.organizationId || !form.appId || !form.name) {
      toast.error("Org, App ve İsim zorunlu"); return;
    }
    const { data } = await api.post<CreatedResponse>("/api-keys", form);
    setJustCreated(data);
    toast.success("API key oluşturuldu. Anahtarı güvenle saklayın.");
    await load();
  };

  const revoke = async (id: string) => {
    await api.post(`/api-keys/${id}/revoke`, {});
    toast.success("Anahtar iptal edildi");
    await load();
  };

  return (
    <div className="grid gap-6">
      <Card>
        <CardHeader><CardTitle>Yeni API Key</CardTitle></CardHeader>
        <CardContent>
          <form className="grid gap-3" onSubmit={create}>
            <div className="grid grid-cols-2 gap-3">
              <div>
                <Label>Organization Id</Label>
                <Input value={form.organizationId} onChange={e => setForm(f => ({ ...f, organizationId: e.target.value }))} placeholder="org guid" />
              </div>
              <div>
                <Label>App Id</Label>
                <Input value={form.appId} onChange={e => setForm(f => ({ ...f, appId: e.target.value }))} placeholder="app guid" />
              </div>
            </div>
            <div>
              <Label>İsim</Label>
              <Input value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} placeholder="örn. backend-worker" />
            </div>
            <div>
              <Label>Geçerlilik (opsiyonel)</Label>
              <Input type="datetime-local" onChange={e => setForm(f => ({ ...f, expiresAt: e.target.value ? new Date(e.target.value).toISOString() : null }))} />
            </div>
            <Button type="submit">Oluştur</Button>
          </form>

          {justCreated && (
            <div className="mt-4 rounded-md border p-3">
              <div className="text-sm font-medium">Plaintext API Key (sadece 1 kez gösterilir)</div>
              <code className="block break-all text-xs mt-1">{justCreated.key}</code>
              <div className="text-xs text-muted-foreground mt-1">Son 4: {justCreated.keyLast4}</div>
            </div>
          )}
        </CardContent>
      </Card>

      <Card>
        <CardHeader><CardTitle>API Keys</CardTitle></CardHeader>
        <CardContent>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="text-left border-b">
                  <th className="py-2">İsim</th>
                  <th>Org</th>
                  <th>App</th>
                  <th>Last4</th>
                  <th>Durum</th>
                  <th>Oluşturuldu</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {data?.items.map(k => (
                  <tr key={k.id} className="border-b">
                    <td className="py-2">{k.name}</td>
                    <td className="font-mono text-xs">{k.organizationId}</td>
                    <td className="font-mono text-xs">{k.appId}</td>
                    <td>{k.keyLast4}</td>
                    <td>{k.isRevoked ? "Revoked" : "Active"}</td>
                    <td>{new Date(k.createdDate).toLocaleString("tr-TR")}</td>
                    <td>
                      {!k.isRevoked && <Button size="sm" variant="destructive" onClick={() => revoke(k.id)}>Revoke</Button>}
                    </td>
                  </tr>
                ))}
                {!data?.items.length && (
                  <tr><td colSpan={7} className="py-4 text-muted-foreground">Kayıt yok</td></tr>
                )}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
