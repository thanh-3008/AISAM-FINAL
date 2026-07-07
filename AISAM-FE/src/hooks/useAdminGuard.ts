"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { isAdmin } from "@/lib/auth";

export function useAdminGuard() {
  const router = useRouter();

  useEffect(() => {
    if (!isAdmin()) {
      router.push("/login");
    }
  }, [router]);

  return { isAdmin: isAdmin() };
}
