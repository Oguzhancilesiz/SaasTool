// src/lib/api.ts
import axios from "axios";

const baseURL = process.env.NEXT_PUBLIC_API_BASE_URL;

export const api = axios.create({ baseURL });

// Token ekle
api.interceptors.request.use((config) => {
  if (typeof window !== "undefined") {
    const token = localStorage.getItem("token");
    if (token) {
      config.headers = config.headers ?? {};
      config.headers.Authorization = `Bearer ${token}`;
    }
  }
  return config;
});

// 401'de login'e gönder
api.interceptors.response.use(
  (res) => res,
  (err) => {
    if (typeof window !== "undefined" && err?.response?.status === 401) {
      localStorage.removeItem("token");
      localStorage.removeItem("expiresAtUtc");
      window.location.href = "/login?returnUrl=" + encodeURIComponent(window.location.pathname);
    }
    return Promise.reject(err);
  }
);
api.interceptors.response.use(
  (res) => res,
  (err) => {
    const title = err?.response?.data?.title;
    const detail = err?.response?.data?.detail;
    const reqId = err?.response?.headers?.["x-request-id"];
    if (title || detail) {
      const suffix = reqId ? ` (ReqId: ${reqId})` : "";
      // import { toast } from "sonner";
      // toast.error(`${title ?? "Hata"}: ${detail ?? ""}${suffix}`);
    }
    if (typeof window !== "undefined" && err?.response?.status === 401) {
      localStorage.removeItem("token");
      localStorage.removeItem("expiresAtUtc");
      window.location.href = "/login?returnUrl=" + encodeURIComponent(window.location.pathname);
    }
    return Promise.reject(err);
  }
);
import { toast } from "sonner";

api.interceptors.response.use(
  r=>r,
  err=>{
    const status = err?.response?.status;
    const title = err?.response?.data?.title;
    const detail = err?.response?.data?.detail;
    const reqId = err?.response?.headers?.["x-request-id"];
    if (status === 429) toast.error("Çok hızlı istek. Lütfen biraz bekleyin.");
    else if (title || detail) toast.error(`${title??"Hata"}: ${detail??""}${reqId?` (ReqId: ${reqId})`:""}`);
    return Promise.reject(err);
  }
);
