// Base64URL decode helper + JWT parse
export function parseJwt<T = any>(token?: string | null): T | null {
  if (!token) return null;
  try {
    const payload = token.split(".")[1];
    // base64url -> base64
    const base64 = payload.replace(/-/g, "+").replace(/_/g, "/");
    const json = decodeURIComponent(
      atob(base64)
        .split("")
        .map((c) => "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2))
        .join("")
    );
    return JSON.parse(json) as T;
  } catch {
    return null;
  }
}
