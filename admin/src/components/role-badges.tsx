"use client";
import { Badge } from "@/components/ui/badge";

export function RoleBadges({ roles }: { roles: string[] }) {
  if (!roles || roles.length === 0) return null;
  return (
    <div className="flex flex-wrap gap-1">
      {roles.map((r) => (
        <Badge key={r} variant="secondary" className="uppercase">
          {r}
        </Badge>
      ))}
    </div>
  );
}
