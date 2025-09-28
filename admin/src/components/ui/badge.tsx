"use client";

import * as React from "react";
import { cn } from "@/lib/utils";

export type BadgeProps = React.HTMLAttributes<HTMLSpanElement> & {
  variant?: "default" | "secondary" | "destructive" | "outline";
};

export function Badge({ className, variant = "default", ...props }: BadgeProps) {
  const base =
    "inline-flex items-center rounded-full border px-2 py-0.5 text-xs font-medium transition-colors";
  const variants: Record<NonNullable<BadgeProps["variant"]>, string> = {
    default:
      "bg-primary text-primary-foreground border-transparent",
    secondary:
      "bg-secondary text-secondary-foreground border-transparent",
    destructive:
      "bg-destructive text-destructive-foreground border-transparent",
    outline:
      "bg-transparent text-foreground border-border",
  };
  return <span className={cn(base, variants[variant], className)} {...props} />;
}
