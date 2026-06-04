import * as React from "react";
import { cn } from "@/lib/utils/cn";

export function Textarea({ className, ...props }: React.TextareaHTMLAttributes<HTMLTextAreaElement>) {
  return (
    <textarea
      className={cn(
        "flex min-h-28 w-full rounded-xl border bg-card px-3 py-2 text-sm outline-none placeholder:text-muted-foreground focus:border-primary",
        className
      )}
      {...props}
    />
  );
}
