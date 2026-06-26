"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/apiClient";

export function useUpdateUserRole() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, role, reason }: { userId: string; role: string; reason: string }) =>
      apiClient(`/admin/users/${userId}/role`, { method: "PATCH", data: { role, reason } }),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["admin", "users"] }); },
  });
}

export function useUpdateUserStatus() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, isActive, reason }: { userId: string; isActive: boolean; reason: string }) =>
      apiClient(`/admin/users/${userId}/status`, { method: "PATCH", data: { isActive, reason } }),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["admin", "users"] }); },
  });
}
