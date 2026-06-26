"use client";

import { useQuery } from "@tanstack/react-query";
import { fetchAdminDashboard } from "@/services/adminService";

export function useAdminDashboard() {
  return useQuery({
    queryKey: ["admin", "dashboard"],
    queryFn: fetchAdminDashboard,
    staleTime: 60_000,
  });
}
