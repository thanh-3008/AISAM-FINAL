"use client";

import { useQuery } from "@tanstack/react-query";
import { fetchAdminUserDetail } from "@/services/adminService";

export function useAdminUserDetail(userId: string) {
  return useQuery({
    queryKey: ["admin", "users", userId],
    queryFn: () => fetchAdminUserDetail(userId),
    enabled: !!userId,
  });
}
