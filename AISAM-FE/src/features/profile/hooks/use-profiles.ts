"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { profileApi, type ProfileInput } from "@/features/profile/api/profile-api";
import { useAuthStore } from "@/stores/auth-store";
import { useProfileStore } from "@/stores/profile-store";

export function useProfiles(search?: string, isDeleted?: boolean) {
  const userId = useAuthStore((state) => state.user?.id);
  return useQuery({
    queryKey: ["profiles", userId, search, isDeleted],
    queryFn: () => profileApi.listByUser(userId as string, search, isDeleted),
    enabled: Boolean(userId)
  });
}

export function useProfileDetail(id: string) {
  return useQuery({
    queryKey: ["profiles", id],
    queryFn: () => profileApi.detail(id),
    enabled: Boolean(id)
  });
}

export function useCreateProfile() {
  const queryClient = useQueryClient();
  const userId = useAuthStore((state) => state.user?.id);
  return useMutation({
    mutationFn: (payload: ProfileInput) => profileApi.create(userId as string, payload),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["profiles", userId] });
    }
  });
}

export function useUpdateProfile(id: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: Partial<ProfileInput>) => profileApi.update(id, payload),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["profiles"] });
      void queryClient.invalidateQueries({ queryKey: ["profiles", id] });
    }
  });
}

export function useDeleteProfile() {
  const queryClient = useQueryClient();
  const activeProfile = useProfileStore((state) => state.activeProfile);
  const clearActiveProfile = useProfileStore((state) => state.clearActiveProfile);
  return useMutation({
    mutationFn: (id: string) => profileApi.delete(id),
    onSuccess: (_, id) => {
      if (activeProfile?.id === id) {
        clearActiveProfile();
      }
      void queryClient.invalidateQueries({ queryKey: ["profiles"] });
    }
  });
}

export function useRestoreProfile() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => profileApi.restore(id),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["profiles"] });
    }
  });
}
