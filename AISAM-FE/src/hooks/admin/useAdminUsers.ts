"use client";

import { useQuery } from "@tanstack/react-query";
import { fetchAdminUsers } from "@/services/adminService";

export function useAdminUsers(params: {
  page?: number; pageSize?: number; searchTerm?: string; sortBy?: string; sortDescending?: boolean; role?: string;
}) {
  return useQuery({
    queryKey: ["admin", "users", params],
    queryFn: () => fetchAdminUsers(params),
    staleTime: 30_000,
  });
}
