"use client";

import { z } from "zod";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useSetAtom } from "jotai";
import { authAtom } from "@/state/auth";
import { api } from "@/lib/api";
import { useRouter } from "next/navigation";
import { toast } from "sonner";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

const schema = z.object({
  email: z.string().email(),
  password: z.string().min(4),
});
type FormValues = z.infer<typeof schema>;

export default function LoginPage() {
  const router = useRouter();
  const setAuth = useSetAtom(authAtom);

  const { register, handleSubmit, formState: { errors, isSubmitting } } =
    useForm<FormValues>({ resolver: zodResolver(schema) });

  const onSubmit = async (values: FormValues) => {
    try {
      const { data } = await api.post<{ accessToken: string; expiresAtUtc: string }>(
        "/auth/login",
        values
      );

      localStorage.setItem("token", data.accessToken);
      localStorage.setItem("expiresAtUtc", data.expiresAtUtc);
      document.cookie = `token=${data.accessToken}; Path=/; SameSite=Lax`;

      setAuth({ token: data.accessToken, expiresAtUtc: data.expiresAtUtc });
      toast.success("Giriş başarılı");
      router.replace("/dashboard");
    } catch (err) {
      toast.error("Giriş başarısız");
      console.error(err);
    }
  };

  return (
    <div className="min-h-screen grid place-items-center p-4">
      <Card className="w-full max-w-sm">
        <CardHeader>
          <CardTitle>Admin Giriş</CardTitle>
        </CardHeader>
        <CardContent>
          <form className="grid gap-4" onSubmit={handleSubmit(onSubmit)}>
            <div className="grid gap-2">
              <Label htmlFor="email">E-posta</Label>
              <Input id="email" type="email" {...register("email")} />
              {errors.email && <p className="text-sm text-red-500">{errors.email.message}</p>}
            </div>
            <div className="grid gap-2">
              <Label htmlFor="password">Şifre</Label>
              <Input id="password" type="password" {...register("password")} />
              {errors.password && <p className="text-sm text-red-500">{errors.password.message}</p>}
            </div>
            <Button disabled={isSubmitting} type="submit">
              {isSubmitting ? "Giriş yapılıyor..." : "Giriş yap"}
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
