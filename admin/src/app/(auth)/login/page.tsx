"use client";
import Link from "next/link";
import { z } from "zod";
import { useEffect, useMemo, useState } from "react";
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
import { Separator } from "@/components/ui/separator";
import { Checkbox } from "@/components/ui/checkbox";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";
import { Badge } from "@/components/ui/badge";

import { Eye, EyeOff, Rocket } from "lucide-react";

const schema = z.object({
  email: z.string().email({ message: "Geçerli bir e-posta gir." }),
  password: z.string().min(4, "Şifre en az 4 karakter olmalı."),
  rememberEmail: z.boolean().optional(),
  rememberPassword: z.boolean().optional(),
});
type FormValues = z.infer<typeof schema>;

// localStorage anahtarları
const LS_EMAIL_KEY = "login:remember:email";
const LS_PASS_KEY = "login:remember:password"; // sadece kullanıcı isterse
const LS_PASS_VAL_KEY = "login:remember:password:value";

export default function LoginPage() {
  const router = useRouter();
  const setAuth = useSetAtom(authAtom);
  const [showPassword, setShowPassword] = useState(false);

  // Kayıtlı verileri oku (ilk render’da)
  const saved = useMemo(() => {
    if (typeof window === "undefined") return { email: "", pass: "" };
    return {
      email: localStorage.getItem(LS_EMAIL_KEY) ?? "",
      pass: localStorage.getItem(LS_PASS_KEY) === "1" ? localStorage.getItem(LS_PASS_VAL_KEY) ?? "" : "",
    };
  }, []);

  const {
    register,
    handleSubmit,
    setValue,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      email: saved.email,
      password: saved.pass,
      rememberEmail: !!saved.email,
      rememberPassword: !!saved.pass,
    },
  });

  // Email/şifreyi isteğe göre sakla
  const rememberEmail = watch("rememberEmail");
  const rememberPassword = watch("rememberPassword");

  useEffect(() => {
    if (typeof window === "undefined") return;
    const currEmail = watch("email");
    if (rememberEmail && currEmail) {
      localStorage.setItem(LS_EMAIL_KEY, currEmail);
    } else {
      localStorage.removeItem(LS_EMAIL_KEY);
    }
  }, [rememberEmail, watch]);

  useEffect(() => {
    if (typeof window === "undefined") return;
    const currPass = watch("password");
    if (rememberPassword && currPass) {
      // UYARI: Şifreyi düz metin olarak depolamak güvenli değildir.
      // Kullanıcı açıkça seçtiyse saklıyoruz.
      localStorage.setItem(LS_PASS_KEY, "1");
      localStorage.setItem(LS_PASS_VAL_KEY, currPass);
    } else {
      localStorage.removeItem(LS_PASS_KEY);
      localStorage.removeItem(LS_PASS_VAL_KEY);
    }
  }, [rememberPassword, watch]);

  const onSubmit = async (values: FormValues) => {
    try {
      const { data } = await api.post<{ accessToken: string; expiresAtUtc: string }>(
        "/auth/login",
        { email: values.email, password: values.password }
      );

      // Token’ı sakla
      localStorage.setItem("token", data.accessToken);
      localStorage.setItem("expiresAtUtc", data.expiresAtUtc);

      // Cookie süresi
      const exp = new Date(data.expiresAtUtc);
      const expires = isNaN(exp.getTime()) ? "" : `; Expires=${exp.toUTCString()}`;
      document.cookie = `token=${data.accessToken}; Path=/; SameSite=Lax${expires}`;

      setAuth({ token: data.accessToken, expiresAtUtc: data.expiresAtUtc });

      // Remember seçeneklerini son kez uygula (formda yazarken de uyguluyoruz zaten)
      if (values.rememberEmail) localStorage.setItem(LS_EMAIL_KEY, values.email);
      if (values.rememberPassword) {
        localStorage.setItem(LS_PASS_KEY, "1");
        localStorage.setItem(LS_PASS_VAL_KEY, values.password);
      }

      toast.success("Giriş başarılı");
      router.replace("/dashboard");
    } catch (err: any) {
      const msg =
        err?.response?.data?.message ||
        err?.message ||
        "Giriş başarısız. Bilgileri kontrol et.";
      toast.error(msg);
      // Güvenlik için yanlış girişte şifreyi temizlemeyi tercih ediyorum.
      setValue("password", "");
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-b from-background to-background/70 grid place-items-center p-4">
      <Card className="w-full max-w-md shadow-xl border-border/60">
        <CardHeader className="space-y-1">
          <div className="flex items-center gap-2 text-muted-foreground">
            <Rocket size={18} />
            <Badge variant="outline" className="uppercase">SaaS Admin</Badge>
          </div>
          <CardTitle className="text-2xl">Admin Giriş</CardTitle>
          <p className="text-sm text-muted-foreground">Paneline dön, faturaları sustur.</p>
        </CardHeader>

        <CardContent>
          <form className="grid gap-4" onSubmit={handleSubmit(onSubmit)}>
            {/* E-posta */}
            <div className="grid gap-2">
              <Label htmlFor="email">E-posta</Label>
              <Input
                id="email"
                type="email"
                autoComplete="email"
                placeholder="sen@firma.com"
                {...register("email")}
              />
              {errors.email && <p className="text-xs text-red-500">{errors.email.message}</p>}
            </div>

            {/* Şifre + göster/gizle */}
            <div className="grid gap-2">
              <div className="flex items-center justify-between">
                <Label htmlFor="password">Şifre</Label>
                <Link href="/forgot-password" className="text-xs text-primary hover:underline">
                  Şifreni mi unuttun?
                </Link>
              </div>

              <div className="relative">
                <Input
                  id="password"
                  type={showPassword ? "text" : "password"}
                  autoComplete="current-password"
                  placeholder="••••••••"
                  {...register("password")}
                  className="pr-9"
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(s => !s)}
                  className="absolute right-2 top-1/2 -translate-y-1/2 rounded-md p-1 hover:bg-muted text-muted-foreground"
                  aria-label={showPassword ? "Şifreyi gizle" : "Şifreyi göster"}
                  tabIndex={0}
                >
                  {showPassword ? <EyeOff size={16} /> : <Eye size={16} />}
                </button>
              </div>
              {errors.password && <p className="text-xs text-red-500">{errors.password.message}</p>}
            </div>

            <Separator className="my-1" />

            {/* Remember seçenekleri */}
            <div className="grid gap-2">
              <label className="flex items-center gap-2 text-sm">
                <Checkbox id="rememberEmail" {...register("rememberEmail")} />
                <span>E-postayı hatırla</span>
              </label>

              <TooltipProvider delayDuration={0}>
                <div className="flex items-center gap-2 text-sm">
                  <Checkbox id="rememberPassword" {...register("rememberPassword")} />
                  <span className="inline-flex items-center gap-1">
                    Şifreyi de hatırla
                    <Tooltip>
                      <TooltipTrigger className="text-muted-foreground hover:underline text-xs">
                        (uyarı)
                      </TooltipTrigger>
                      <TooltipContent className="max-w-xs text-xs">
                        Şifreyi tarayıcıda düz metin olarak saklamak güvenli değildir.
                        Sadece kişisel cihazda ve kısa süreli kullan.
                      </TooltipContent>
                    </Tooltip>
                  </span>
                </div>
              </TooltipProvider>
            </div>

            <Button disabled={isSubmitting} type="submit" className="mt-2">
              {isSubmitting ? "Giriş yapılıyor..." : "Giriş yap"}
            </Button>
          </form>

          {/* Alt bilgi */}
          <div className="mt-4 text-xs text-muted-foreground">
            Bu sayfa çerez kullanır. Girişte alınan erişim belirtecinin süresi otomatik ayarlanır.
          </div>
        </CardContent>
      </Card>
    </div>
  );
}