"use client";

import { useEffect } from "react";
import { useToastStore } from "@/stores/toast-store";

export function ToastViewport() {
  const items = useToastStore((state) => state.items);
  const remove = useToastStore((state) => state.remove);

  useEffect(() => {
    const timers = items.map((item) =>
      window.setTimeout(() => {
        remove(item.id);
      }, 3500)
    );

    return () => {
      timers.forEach((timer) => window.clearTimeout(timer));
    };
  }, [items, remove]);

  return (
    <div className="fixed right-4 top-4 z-50 flex w-full max-w-sm flex-col gap-3">
      {items.map((item) => (
        <div
          key={item.id}
          className={`rounded-2xl border p-4 shadow-panel ${
            item.tone === "error"
              ? "border-destructive/20 bg-destructive/5"
              : item.tone === "success"
                ? "border-primary/20 bg-primary/5"
                : "border-border bg-card"
          }`}
        >
          <p className="text-sm font-semibold">{item.title}</p>
          {item.description ? <p className="mt-1 text-sm text-muted-foreground">{item.description}</p> : null}
        </div>
      ))}
    </div>
  );
}
