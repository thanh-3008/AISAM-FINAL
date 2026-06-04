"use client";

import { usePathname, useRouter } from "next/navigation";
import { PropsWithChildren, useEffect } from "react";
import { useAuthStore } from "@/stores/auth-store";

export function ProtectedRoute({ children }: PropsWithChildren) {
  const router = useRouter();
  const hydrated = useAuthStore((state) => state.hydrated);
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated());
  const pathname = usePathname();

  useEffect(() => {
    if (!hydrated) {
      return;
    }
    if (!isAuthenticated) {
      router.replace(`/login?next=${encodeURIComponent(pathname)}`);
    }
  }, [hydrated, isAuthenticated, pathname, router]);

  if (!hydrated || !isAuthenticated) {
    return <div className="rounded-2xl bg-card p-6 shadow-panel">Checking session...</div>;
  }

  return <>{children}</>;
}

export function PublicOnlyRoute({ children }: PropsWithChildren) {
  const router = useRouter();
  const hydrated = useAuthStore((state) => state.hydrated);
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated());

  useEffect(() => {
    if (hydrated && isAuthenticated) {
      router.replace("/dashboard");
    }
  }, [hydrated, isAuthenticated, router]);

  if (!hydrated) {
    return <div className="min-h-screen bg-background" />;
  }

  return <>{children}</>;
}
