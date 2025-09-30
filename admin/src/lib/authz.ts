// Kullanıcı rollerini JWT'den normalize et
export function extractRoles(payload?: any): string[] {
  if (!payload) return [];
  const r = payload["role"] ?? payload["roles"] ?? [];
  if (Array.isArray(r)) return r.map(String);
  if (typeof r === "string") return [r];
  return [];
}

// Basit permission matrisi (client-side). İleride server’dan getiririz.
const ALL = [
  "Dashboard.View","Dashboard.Finance",
  "Apps.Read","Plans.Read","Features.Read","Organizations.Read","Customers.Read",
  "Subscriptions.Read","Usage.Read","Invoices.Read","Payments.Read",
];

const BY_ROLE: Record<string, string[]> = {
  SuperAdmin: ALL,
  OrgAdmin: [
    "Dashboard.View","Dashboard.Finance",
    "Apps.Read","Plans.Read","Features.Read","Organizations.Read","Customers.Read",
    "Subscriptions.Read","Usage.Read","Invoices.Read","Payments.Read",
  ],
  Analyst: [
    "Dashboard.View","Dashboard.Finance",
    "Apps.Read","Plans.Read","Features.Read","Organizations.Read","Customers.Read",
    "Subscriptions.Read","Usage.Read","Invoices.Read",
  ],
  Support: [
    "Dashboard.View",
    "Customers.Read","Subscriptions.Read","Invoices.Read","Usage.Read",
  ],
};

export function rolesToPermissions(roles: string[]): string[] {
  const set = new Set<string>();
  roles.forEach(r => (BY_ROLE[r] ?? []).forEach(p => set.add(p)));
  return [...set];
}

export function hasPerm(perms: string[], need: string | string[]) {
  const needArr = Array.isArray(need) ? need : [need];
  return needArr.every(n => perms.includes(n));
}
