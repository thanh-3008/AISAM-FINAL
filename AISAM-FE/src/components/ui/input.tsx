import * as React from "react";
import { cn } from "@/lib/utils/cn";

export function Input({ className, ...props }: React.InputHTMLAttributes<HTMLInputElement>) {
  return (
    <input
      className={cn(
        "flex h-11 w-full rounded-xl border bg-card px-3 py-2 text-sm outline-none ring-0 placeholder:text-muted-foreground focus:border-primary",
        className
      )}
      {...props}
    />
  );
}
